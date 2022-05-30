using System.Data;
using System.Text;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.FileLocations;
using MarlinCompiler.Common.Messages;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Symbols.Kinds;
using MarlinCompiler.Frontend.Lexing;

namespace MarlinCompiler.Frontend.Parsing;

/// <summary>
/// Parser class.
/// Naming conventions:
/// - Expect... - method for getting whole nodes
/// - Grab... - method for getting non-nodes, e.g. a type name
/// - Require... - method that adds an error if something is missing (e.g. semicolon)
/// </summary>
public sealed class Parser
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="tokens">The tokens to work with.</param>
    /// <param name="filePath">Path to the source file. Used solely for error reporting.</param>
    public Parser(Tokens tokens, string filePath)
    {
        MessageCollection            = new MessageCollection();
        _tokens                      = tokens;
        _path                        = filePath;
        _moduleName                  = "<global>";
        _compilationUnitDependencies = new List<(string, TokenLocation)>();
    }

    /// <summary>
    /// All parser messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    /// <summary>
    /// A reference to the current parse operation token list.
    /// </summary>
    private readonly Tokens _tokens;

    /// <summary>
    /// A filename to be used for error reporting.
    /// </summary>
    private readonly string _path;

    /// <summary>
    /// The current module's name. There can be only one module per file.
    /// </summary>
    private string _moduleName;

    /// <summary>
    /// The dependencies for this compilation unit.
    /// </summary>
    private readonly List<(string, TokenLocation)> _compilationUnitDependencies;

    /// <summary>
    /// Starts the parse operation.
    /// </summary>
    /// <returns>The compilation unit node or null when the token source is empty.</returns>
    public CompilationUnitNode? Parse()
    {
        return !_tokens.HasNext
                   ? null
                   : ExpectCompilationUnit();
    }

    /// <summary>
    /// Tries to parse a compilation unit. Does not throw parse exceptions.
    /// </summary>
    private CompilationUnitNode ExpectCompilationUnit()
    {
        _compilationUnitDependencies.AddRange(GrabUsingDirectives());
        string name = GrabModuleDirective();
        _moduleName = name;

        // Create node & symbol
        CompilationUnitNode node = new(name, _compilationUnitDependencies.ToArray());
        ModuleSymbol symbol = new(node);
        SymbolTable scope = new(null, symbol);
        symbol.SymbolTable = scope;
        List<Node> children = new();

        try
        {
            while (_tokens.HasNext)
            {
                try
                {
                    Node? child = ExpectTypeDefinition(scope);

                    if (child != null)
                    {
                        children.Add(child);
                    }
                }
                catch (ParseException ex)
                {
                    LogErrorAndRecover(ex, true);
                }
            }
        }
        catch (CancelParsingException ex)
        {
            MessageCollection.Info(MessageId.ParsingCancelled, $"Parsing cancelled for file {_path}: {ex.Message}", null);
        }

        node.Children.AddRange(children);

        foreach (Node childNode in children)
        {
            if (!childNode.HasMetadata)
            {
                throw new NoNullAllowedException("Type node does not have metadata.");
            }

            // Types are scopes, so we are expecting a symbol table
            SymbolTable typeTable = childNode.GetMetadata<SymbolTable>();
            try
            {
                scope.AddSymbol(typeTable);
            }
            catch (SymbolNameAlreadyExistsException ex)
            {
                MessageCollection.Error(MessageId.SymbolAlreadyDefined, ex.Message, childNode.Location);
            }
        }
        node.SetMetadata(scope);

        return node;
    }

    /// <summary>
    /// Expects a type definition.
    /// </summary>
    /// <returns>Null for EOF, otherwise the node of the type def.</returns>
    private TypeDefinitionNode? ExpectTypeDefinition(SymbolTable scope)
    {
        Token? next = _tokens.PeekNextTokenBySkipping(TokenType.Modifier, 0);

        if (next == null)
        {
            return null;
        }

        return next.Type switch
        {
            TokenType.Class  => ExpectClassDefinition(scope),
            TokenType.Struct => ExpectStructDefinition(),
            TokenType.Extern => ExpectExternTypeDefinition(),
            _ => throw new ParseException(
                     MessageId.UnexpectedToken,
                     $"Expected type definition, got {next.Type} ('{next.Value}')",
                     next
                 )
        };
    }

    /// <summary>
    /// Expects a type body, including the braces.
    /// </summary>
    private ContainerNode ExpectTypeBody(SymbolTable typeScope)
    {
        ContainerNode bodyNode = new();

        Require(TokenType.LeftBrace);
        while (!_tokens.NextIsOfType(TokenType.RightBrace))
        {
            try
            {
                Node child = ExpectTypeMember(typeScope);
                bodyNode.Children.Add(child);
            }
            catch (ParseException ex)
            {
                LogErrorAndRecover(ex, true);
            }
        }
        Require(TokenType.RightBrace);

        return bodyNode;
    }

    /// <summary>
    /// Expects a body for an extern type, including the braces.
    /// An extern body consists of method mappings.
    /// </summary>
    private ContainerNode ExpectExternTypeBody(SymbolTable scope)
    {
        ContainerNode bodyNode = new();

        Require(TokenType.LeftBrace);
        while (!_tokens.NextIsOfType(TokenType.RightBrace))
        {
            try
            {
                Node child = ExpectExternMethod(scope);

                bodyNode.Children.Add(child);
            }
            catch (ParseException ex)
            {
                LogErrorAndRecover(ex, true);
            }
        }
        Require(TokenType.RightBrace);

        return bodyNode;
    }

    /// <summary>
    /// Expects a type member (i.e. property or method)
    /// </summary>
    private Node ExpectTypeMember(SymbolTable typeScope)
    {
        // We gotta look a bit ahead to understand what this actually is
        // We'll cheat: we'll make a new parser and a new Tokens instance
        // this way we can advance without really screwing up our own state
        // bonus: we can access all private methods!
        Parser tempParser = CreateSubParser();

        // Type members are method declarations and property declarations
        // Both start off with modifiers, type name and then the name of the member
        // but then methods have a left paren, which is what we'll use to determine
        // which one we have
        tempParser.GrabModifiers(); // don't care about those

        // If the next token is the 'constructor' keyword, we have a constructor!
        if (tempParser._tokens.NextIsOfType(TokenType.Constructor))
        {
            // Constructor!
            return ExpectConstructor(typeScope);
        }

        tempParser.ExpectTypeName(new SymbolTable(null));     // don't care about this either
        tempParser.GrabNextByExpecting(TokenType.Identifier); // nope

        // and now we can just check if the next token is a left paren
        return tempParser._tokens.NextIsOfType(TokenType.LeftParen)
                   ? ExpectMethodDeclaration(new SymbolTable(null))
                   : ExpectPropertyDeclaration(new SymbolTable(null));
    }

    /// <summary>
    /// Expects an extern method or constructor mapping.
    /// </summary>
    private Node ExpectExternMethod(SymbolTable typeScope)
    {
        string[] modifiers = GrabModifiers();
        TypeReferenceNode type;

        string? name = null;

        if (_tokens.NextIsOfType(TokenType.Constructor))
        {
            type = new TypeReferenceNode(ExternMethodSymbol.ConstructorTypeName, false, Array.Empty<TypeReferenceNode>());
            _tokens.Skip(); // constructor
        }
        else
        {
            type = ExpectTypeName(typeScope);
            name = GrabNextByExpecting(TokenType.Identifier);
        }

        Token nameToken = _tokens.CurrentToken;
        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "protected", "private", "static");
        VariableNode[] expectedArgs = GrabTupleTypeDefinition(typeScope);

        GrabNextByExpecting(TokenType.Arrow);
        GrabNextByExpecting(TokenType.At);

        GrabNextByExpecting(TokenType.Identifier);
        ExpressionNode[] passedArgs = GrabTupleValues(typeScope);

        Require(TokenType.Semicolon);

        GetAccessibility getAccessibility = VisibilityFromModifiers(modifiers, MessageId.AlwaysSpecifyMemberVisibility);
        bool isStatic = modifiers.Contains("static");
        ExternMethodNode node = new(
            getAccessibility,
            type,
            name,
            isStatic,
            expectedArgs,
            passedArgs
        )
        {
            Location = nameToken.Location
        };

        // Symbol
        ExternMethodSymbol symbol = new(node);
        node.SetMetadata(symbol);

        return node;
    }

    /// <summary>
    /// Expects a class definition.
    /// </summary>
    /// <remarks><see cref="moduleScope"/> is necessary for base class</remarks>
    private ClassTypeDefinitionNode ExpectClassDefinition(SymbolTable moduleScope)
    {
        string[] modifiers = GrabModifiers();

        GrabNextByExpecting(TokenType.Class);

        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        GetAccessibility accessibility = VisibilityFromModifiers(modifiers, MessageId.AlwaysSpecifyTypeVisibility);

        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "static");

        // Generics
        // Most classes don't have generic params, we'll try to waste as little memory as possible
        List<string> genericTypeParamNames = new(0);
        if (_tokens.NextIsOfType(TokenType.LeftAngle))
        {
            do
            {
                _tokens.Skip(); // < or comma
                genericTypeParamNames.Add(GrabNextByExpecting(TokenType.Identifier));
            }
            while (_tokens.NextIsOfType(TokenType.Comma));
            Require(TokenType.RightAngle);
        }

        // Inheritance
        TypeReferenceNode? baseClass = null;
        if (_tokens.NextIsOfType(TokenType.Colon))
        {
            _tokens.Skip(); // colon
            baseClass = ExpectTypeName(moduleScope);

            if (baseClass.IsNullable)
            {
                MessageCollection.Error(
                    MessageId.NullableTypeInheritance,
                    "Cannot inherit from nullable type. Please remove the '?' from the base class name.",
                    baseClass.Location
                );
            }
        }
        else if (_moduleName + "::Object" != "std::Object") // don't make the base obj inherit from itself lol
        {
            baseClass = new TypeReferenceNode("std::Object", false, Array.Empty<TypeReferenceNode>())
                { Location = nameToken.Location };
        }

        bool isStatic = modifiers.Contains("static");
        ClassTypeDefinitionNode classNode = new(
            name,
            _moduleName,
            accessibility,
            isStatic,
            baseClass,
            genericTypeParamNames.ToArray()
        )
        {
            Location = nameToken.Location
        };

        // Create symbol
        ClassTypeSymbol symbol = new(classNode);
        SymbolTable scope = new(null, symbol);
        symbol.SymbolTable = scope;

        // Add type body
        ContainerNode typeBody = ExpectTypeBody(scope);
        classNode.Children.AddRange(typeBody);

        foreach (string genericParam in genericTypeParamNames)
        {
            scope.AddSymbol(new GenericParamTypeSymbol(genericParam, symbol));
        }

        foreach (Node childNode in typeBody)
        {
            // Types have members, which are symbols (properties) OR tables (methods)

            try
            {
                if (childNode.MetadataIs<ISymbol>())
                {
                    ISymbol memberSymbol = childNode.GetMetadata<ISymbol>();
                    scope.AddSymbol(memberSymbol);
                }
                else if (childNode.MetadataIs<SymbolTable>())
                {
                    SymbolTable memberSymbolTable = childNode.GetMetadata<SymbolTable>();
                    scope.AddSymbol(memberSymbolTable);
                }
                else
                {
                    throw new InvalidOperationException("Class member has invalid metadata.");
                }
            }
            catch (SymbolNameAlreadyExistsException ex)
            {
                MessageCollection.Error(MessageId.SymbolAlreadyDefined, ex.Message, childNode.Location);
            }
        }

        classNode.SetMetadata(scope);

        return classNode;
    }

    /// <summary>
    /// Expects a struct definition.
    /// </summary>
    private StructTypeDefinitionNode ExpectStructDefinition()
    {
        string[] modifiers = GrabModifiers();

        GrabNextByExpecting(TokenType.Struct);

        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        GetAccessibility accessibility = VisibilityFromModifiers(modifiers, MessageId.AlwaysSpecifyTypeVisibility);

        ApplyModifierFilters(modifiers, nameToken, "public", "internal");

        StructTypeDefinitionNode structNode = new(
            name,
            _moduleName,
            accessibility
        )
        {
            Location = nameToken.Location
        };

        // Create symbol
        StructTypeSymbol symbol = new(structNode);
        SymbolTable scope = new(null, symbol);
        symbol.SymbolTable = scope;

        // Type body
        ContainerNode typeBody = ExpectTypeBody(scope);
        structNode.Children.AddRange(typeBody);

        foreach (Node childNode in typeBody)
        {
            try
            {
                // Types have members, which are symbols (properties) OR tables (methods)
                if (childNode.MetadataIs<ISymbol>())
                {
                    ISymbol memberSymbol = childNode.GetMetadata<ISymbol>();
                    scope.AddSymbol(memberSymbol);
                }
                else if (childNode.MetadataIs<SymbolTable>())
                {
                    SymbolTable memberSymbolTable = childNode.GetMetadata<SymbolTable>();
                    scope.AddSymbol(memberSymbolTable);
                }
                else
                {
                    throw new InvalidOperationException("Struct member has invalid metadata.");
                }
            }
            catch (SymbolNameAlreadyExistsException ex)
            {
                MessageCollection.Error(MessageId.SymbolAlreadyDefined, ex.Message, childNode.Location);
            }
        }
        structNode.SetMetadata(scope);

        return structNode;
    }

    /// <summary>
    /// Expects an external struct mapping.
    /// </summary>
    private ExternTypeDefinitionNode ExpectExternTypeDefinition()
    {
        string[] modifiers = GrabModifiers();

        GrabNextByExpecting(TokenType.Extern);

        string name = GrabNextByExpecting(TokenType.Identifier);
        string? llvmTypeName = null;
        Token nameToken = _tokens.CurrentToken;
        GetAccessibility accessibility = VisibilityFromModifiers(modifiers, MessageId.AlwaysSpecifyTypeVisibility);
        bool isStatic = modifiers.Contains("static");

        if (!isStatic)
        {
            if (_tokens.NextIsOfType(TokenType.At))
            {
                _tokens.Skip(); // @
                llvmTypeName = GrabNextByExpecting(TokenType.Identifier);
            }
            else
            {
                MessageCollection.Error(
                    MessageId.ExpectedLlvmTypeName,
                    $"Expected LLVM type name for non-static extern type {name}",
                    nameToken.Location
                );
            }
        }

        ExternTypeDefinitionNode externTypeNode = new(name, _moduleName, accessibility, isStatic, llvmTypeName)
        {
            Location = nameToken.Location
        };

        // Create symbol
        ExternTypeSymbol symbol = new(externTypeNode);
        SymbolTable scope = new(null, symbol);
        symbol.SymbolTable = scope;

        // Type body
        ContainerNode typeBody = ExpectExternTypeBody(scope);
        externTypeNode.Children.AddRange(typeBody.Children);

        foreach (Node childNode in typeBody)
        {
            // Types have members, which are symbols (properties) OR tables (methods)
            try
            {
                if (childNode.MetadataIs<ISymbol>())
                {
                    ISymbol memberSymbol = childNode.GetMetadata<ISymbol>();
                    scope.AddSymbol(memberSymbol);
                }
                else if (childNode.MetadataIs<SymbolTable>())
                {
                    SymbolTable memberSymbolTable = childNode.GetMetadata<SymbolTable>();
                    scope.AddSymbol(memberSymbolTable);
                }
                else
                {
                    throw new InvalidOperationException("Extern type member has invalid metadata.");
                }
            }
            catch (SymbolNameAlreadyExistsException ex)
            {
                MessageCollection.Error(MessageId.SymbolAlreadyDefined, ex.Message, childNode.Location);
            }
        }
        externTypeNode.SetMetadata(scope);

        return externTypeNode;
    }

    /// <summary>
    /// Expects a constructor declaration.
    /// </summary>
    private ConstructorDeclarationNode ExpectConstructor(SymbolTable typeScope)
    {
        string[] modifiers = GrabModifiers();
        GrabNextByExpecting(TokenType.Constructor);
        Token ctorToken = _tokens.CurrentToken;

        ApplyModifierFilters(modifiers, ctorToken, "public", "internal", "protected", "private");
        VariableNode[] parameters = GrabTupleTypeDefinition(typeScope);

        GetAccessibility accessibility = VisibilityFromModifiers(modifiers, MessageId.AlwaysSpecifyMemberVisibility);

        ConstructorDeclarationNode node = new(
            accessibility,
            parameters
        )
        {
            Location = ctorToken.Location
        };

        ConstructorSymbol symbol = new(node);
        SymbolTable scope = new(null, symbol);
        foreach (VariableNode param in parameters)
        {
            VariableSymbol variableSymbol = new(param);
            param.SetMetadata(variableSymbol);

            try
            {
                scope.AddSymbol(variableSymbol);
            }
            catch (SymbolNameAlreadyExistsException ex)
            {
                MessageCollection.Error(MessageId.SymbolAlreadyDefined, ex.Message, param.Location);
            }
        }
        node.SetMetadata(scope);

        // Body!!!
        ContainerNode body = ExpectStatementBody(scope, false);
        node.Children.AddRange(body);

        return node;
    }

    /// <summary>
    /// Expects a method declaration. 
    /// </summary>
    private MethodDeclarationNode ExpectMethodDeclaration(SymbolTable typeScope)
    {
        string[] modifiers = GrabModifiers();
        TypeReferenceNode type = ExpectTypeName(typeScope);
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "protected", "private", "static");
        VariableNode[] parameters = GrabTupleTypeDefinition(typeScope);

        GetAccessibility accessibility = VisibilityFromModifiers(modifiers, MessageId.AlwaysSpecifyMemberVisibility);
        bool isStatic = modifiers.Contains("static");

        MethodDeclarationNode node = new(
            accessibility,
            type,
            name,
            isStatic,
            parameters
        )
        {
            Location = nameToken.Location
        };

        MethodSymbol symbol = new(node);
        SymbolTable scope = new(null, symbol);
        foreach (VariableNode param in parameters)
        {
            VariableSymbol variableSymbol = new(param);
            param.SetMetadata(variableSymbol);

            try
            {
                scope.AddSymbol(variableSymbol);
            }
            catch (SymbolNameAlreadyExistsException ex)
            {
                MessageCollection.Error(MessageId.SymbolAlreadyDefined, ex.Message, param.Location);
            }
        }
        node.SetMetadata(scope);

        // Body!!!!
        ContainerNode body = ExpectStatementBody(scope, false);
        node.Children.AddRange(body.Children);

        return node;
    }

    /// <summary>
    /// Expects a collection of statements surrounded by curly braces.
    /// </summary>
    /// <param name="scope">The symbol table to populate.</param>
    /// <param name="insideLoop">Are we inside a loop? This enables continue and break statements.</param>
    /// TODO Implement insideLoop
    private ContainerNode ExpectStatementBody(SymbolTable scope, bool insideLoop)
    {
        ContainerNode node = new();

        Require(TokenType.LeftBrace);
        while (!_tokens.NextIsOfType(TokenType.RightBrace))
        {
            try
            {
                Node child = ExpectStatement(scope, insideLoop);

                node.Children.Add(child);

                if (child.HasMetadata)
                {
                    try
                    {
                        if (child.MetadataIs<ISymbol>())
                        {
                            ISymbol symbol = child.GetMetadata<ISymbol>();
                            scope.AddSymbol(symbol);
                        }
                        else if (child.MetadataIs<SymbolTable>())
                        {
                            SymbolTable table = child.GetMetadata<SymbolTable>();
                            scope.AddSymbol(table);
                        }
                    }
                    catch (SymbolNameAlreadyExistsException ex)
                    {
                        MessageCollection.Error(MessageId.SymbolAlreadyDefined, ex.Message, child.Location);
                    }
                }
            }
            catch (ParseException ex)
            {
                LogErrorAndRecover(ex, false);
            }
        }

        Require(TokenType.RightBrace);

        node.SetMetadata(scope);

        return node;
    }

    /// <summary>
    /// Expects a property declaration.
    /// </summary>
    private Node ExpectPropertyDeclaration(SymbolTable typeScope)
    {
        string[] modifiers = GrabModifiers();
        TypeReferenceNode type = ExpectTypeName(typeScope);
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;

        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "protected", "private", "static");
        GetAccessibility get = VisibilityFromModifiers(modifiers, MessageId.AlwaysSpecifyMemberVisibility);
        SetAccessibility set = SetAccessibility.NoModify;
        ExpressionNode? value = null;

        if (_tokens.NextIsOfType(TokenType.Arrow))
        {
            _tokens.Skip(); // ->
            Token? peek = _tokens.PeekToken();

            bool assignedGet = false;
            bool assignedSet = false;
            while (true)
            {
                if (peek == null)
                {
                    throw new CancelParsingException("Premature EOF");
                }

                string currentAccessibility = get.ToString().ToLower();

                if (peek.Type == TokenType.Modifier)
                {
                    currentAccessibility = _tokens.GrabToken()!.Value;

                    if (currentAccessibility is not ("public" or "private" or "internal"))
                    {
                        MessageCollection.Error(
                            MessageId.InapplicableModifier,
                            $"Unexpected modifier {currentAccessibility}",
                            peek.Location
                        );
                        currentAccessibility = "internal";
                    }

                    peek = _tokens.PeekToken();
                    if (peek == null)
                    {
                        throw new CancelParsingException("Premature EOF");
                    }
                }

                if (peek.Type == TokenType.Get)
                {
                    if (assignedGet)
                    {
                        MessageCollection.Error(MessageId.RepeatedModifier, "Repeated get specifier", peek.Location);
                    }
                    assignedGet = true;

                    if (!String.Equals(currentAccessibility, get.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageCollection.Error(
                            MessageId.InconsistentAccessibilityModifiers,
                            "Get accessibility must be the same as the accessibility of the property itself.",
                            peek.Location
                        );
                    }

                    Enum.TryParse(currentAccessibility, true, out get);

                    _tokens.Skip(); // get

                    if (_tokens.NextIsOfType(TokenType.Comma))
                    {
                        _tokens.Skip(); // ,
                        peek = _tokens.PeekToken();
                    }
                    else
                    {
                        break;
                    }
                }
                else if (peek.Type == TokenType.Set)
                {
                    if (assignedSet)
                    {
                        MessageCollection.Error(MessageId.RepeatedModifier, "Repeated set specifier", peek.Location);
                    }
                    assignedSet = true;
                    Enum.TryParse(currentAccessibility, true, out set);

                    if ((short) set > (short) get)
                    {
                        MessageCollection.Error(
                            MessageId.InconsistentAccessibilityModifiers,
                            "Get accessibility cannot be more restrictive than set",
                            peek.Location
                        );
                    }

                    _tokens.Skip(); // set

                    if (_tokens.NextIsOfType(TokenType.Comma))
                    {
                        _tokens.Skip(); // ,
                        peek = _tokens.PeekToken();
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    throw new ParseException(MessageId.UnexpectedToken, "Expected specifier", peek);
                }
            }

            if (!assignedSet)
            {
                set = SetAccessibility.NoModify;
            }
            else if (!assignedGet && assignedSet)
            {
                MessageCollection.Error(
                    MessageId.MissingAccessibilityModifier,
                    "Cannot have only set specifier (you must add get as well)",
                    nameToken.Location
                );
            }

            if (assignedGet && !assignedSet)
            {
                MessageCollection.Warn(
                    MessageId.RedundantPropertyAccessibilityModifier,
                    "Don't add redundant get specifiers (missing set specifier means readonly anyway)",
                    nameToken.Location
                );
            }
        }

        if (_tokens.NextIsOfType(TokenType.Assign))
        {
            _tokens.Skip(); // =
            value = ExpectExpression(typeScope);
        }

        Require(TokenType.Semicolon);

        bool isStatic = modifiers.Contains("static");
        PropertyNode node = new(
            type,
            name,
            isStatic,
            value,
            get,
            set
        )
        {
            Location = nameToken.Location
        };

        PropertySymbol symbol = new(node);
        node.SetMetadata(symbol);

        return node;
    }

    /// <summary>
    /// Expects a local variable declaration.
    /// </summary>
    private VariableNode ExpectLocalVariableDeclaration(SymbolTable scope)
    {
        bool mutable = _tokens.NextIsOfType(TokenType.Mutable);
        if (mutable)
        {
            _tokens.Skip(); // mut
        }

        TypeReferenceNode type = ExpectTypeName(scope);
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        ExpressionNode? value = null;

        if (_tokens.NextIsOfType(TokenType.Assign))
        {
            _tokens.Skip(); // =
            value = ExpectExpression(scope);
        }

        Require(TokenType.Semicolon);

        LocalVariableDeclarationNode node = new(
            type,
            name,
            mutable,
            value
        )
        {
            Location = nameToken.Location
        };


        VariableSymbol symbol = new(node)
        {
            IsInitialized = value != null
        };
        node.SetMetadata(symbol);

        return node;
    }

    /// <summary>
    /// Expects a return statement.
    /// </summary>
    /// <param name="scope"></param>
    private ReturnStatementNode ExpectReturnStatement(SymbolTable scope)
    {
        _tokens.GrabToken(); // return

        ExpressionNode? value = null;

        if (!_tokens.NextIsOfType(TokenType.Semicolon))
        {
            value = ExpectExpression(scope);
        }

        Require(TokenType.Semicolon);

        return new ReturnStatementNode(value)
        {
            Location = _tokens.CurrentToken.Location
        };
    }

    /// <summary>
    /// Expects any statement.
    /// </summary>
    /// <param name="scope">Current scope.</param>
    /// <param name="insideLoop">Is this a statement inside a loop? This enables continue and break statements.</param>
    private Node ExpectStatement(SymbolTable scope, bool insideLoop)
    {
        // Statements:
        //   statement block
        //   local variable declaration         type identifier (assign expr)? semicolon
        //   variable assignment                (accessPath dot)? identifier assign expr semicolon
        //   method call                        (accessPath dot)? identifier tuple semicolon
        //   return statement                   return (expr)? semicolon

        // Empty statement
        if (_tokens.NextIsOfType(TokenType.Semicolon))
        {
            return new EmptyStatementNode
            {
                Location = _tokens.GrabToken()!.Location
            };
        }

        // Statement block
        if (_tokens.NextIsOfType(TokenType.LeftBrace))
        {
            SymbolTable newScope = new(null);
            return ExpectStatementBody(newScope, insideLoop);
        }

        // Return statement
        if (_tokens.NextIsOfType(TokenType.Return))
        {
            return ExpectReturnStatement(scope);
        }

        // Variable declaration
        try
        {
            Parser parser = CreateSubParser();

            if (parser._tokens.NextIsOfType(TokenType.Mutable))
            {
                parser._tokens.Skip();
            }

            parser.ExpectTypeName(new SymbolTable(null)); // var type
            parser.GrabNextByExpecting(TokenType.Identifier);
            if (parser.MessageCollection.HasFatalErrors)
            {
                throw new FormatException();
            }

            Token? next = parser._tokens.GrabToken();
            if (next == null)
            {
                throw new CancelParsingException("Premature EOF");
            }
            if (next.Type is TokenType.Semicolon or TokenType.Assign)
            {
                return ExpectLocalVariableDeclaration(scope); // use regular parser, not sub-parser!
            }
        }
        catch (FormatException)
        {
        }

        // Try getting an expression chain (method call/assignment)
        try
        {
            ExpressionNode expr = ExpectExpression(new SymbolTable(null));

            if (expr is MethodCallNode or VariableAssignmentNode)
            {
                Require(TokenType.Semicolon);
                return expr;
            }
        }
        catch (ParseException)
        {
        }

        // Cannot figure out what this is
        throw new ParseException(
            MessageId.UnexpectedToken,
            $"Expected statement, got {_tokens.PeekToken()!.Type}",
            _tokens.GrabToken()!
        );
    }

    /// <summary>
    /// Expects a variable assignment.
    /// </summary>
    private VariableAssignmentNode ExpectVariableAssignment(SymbolTable scope)
    {
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        GrabNextByExpecting(TokenType.Assign);
        ExpressionNode value = ExpectExpression(scope);
        return new VariableAssignmentNode(name, value) { Location = nameToken.Location };
    }

    /// <summary>
    /// Expects a method call.
    /// </summary>
    private MethodCallNode ExpectMethodCall(SymbolTable scope)
    {
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        ExpressionNode[] args = GrabTupleValues(scope);

        bool isNativeCall = _tokens.NextIsOfType(TokenType.At);
        return new MethodCallNode(name, isNativeCall, args)
        {
            Location = nameToken.Location
        };
    }

    /// <summary>
    /// Expects any expression.
    /// </summary>
    private ExpressionNode ExpectExpression(SymbolTable scope, bool doNotEvaluateOperators = false)
    {
        Token? next = _tokens.PeekToken();
        if (next == null)
        {
            throw new CancelParsingException("Expected expression, got EOF");
        }

        ExpressionNode expr;

        switch (next.Type)
        {
            case TokenType.Null:
                expr = new NullNode()
                {
                    Location = _tokens.GrabToken()!.Location
                };
                break;

            case TokenType.LeftParen:
                Require(TokenType.LeftParen);
                expr = ExpectExpression(scope);
                Require(TokenType.RightParen);
                break;

            case TokenType.Integer:
                expr = new IntegerNode(Int32.Parse(_tokens.GrabToken()!.Value)) { Location = next.Location };
                break;

            case TokenType.Decimal:
                throw new NotImplementedException();

            case TokenType.New:
                expr = ExpectNew(scope);
                break;

            case TokenType.Identifier:
                Token? peek = _tokens.PeekToken(2);
                if (peek == null)
                {
                    throw new CancelParsingException("Expected expression, got EOF");
                }

                expr = peek.Type switch
                {
                    TokenType.DoubleColon => ExpectTypeName(scope),
                    TokenType.LeftParen   => ExpectMethodCall(scope),
                    TokenType.Assign      => ExpectVariableAssignment(scope),
                    _ => new MemberAccessNode(_tokens.GrabToken()!.Value)
                    {
                        Location = peek.Location
                    }
                };

                break;

            default:
                throw new ParseException(MessageId.UnexpectedToken, $"Expected expression, got {next.Type}", next);
        }

        return doNotEvaluateOperators
                   ? expr
                   : ExpectExpressionRhs(scope, expr, 0);
    }

    /// <summary>
    /// Complements ExpectExpression. Takes care of operation precedence.
    /// </summary>
    /// <remarks>https://en.wikipedia.org/wiki/Operator-precedence_parser#Pseudocode</remarks>
    private ExpressionNode ExpectExpressionRhs(SymbolTable scope, ExpressionNode left, int minimumPrecedence)
    {
        Token? peek = _tokens.PeekToken();
        if (peek == null)
        {
            throw new CancelParsingException("Premature EOF");
        }

        // MEMBER ACCESS
        if (peek.Type == TokenType.Dot)
        {
            while (_tokens.NextIsOfType(TokenType.Dot))
            {
                peek = _tokens.GrabToken()!; // .
                ExpressionNode right = ExpectExpression(scope, true);

                if (right is not IndexableExpressionNode node)
                {
                    throw new ParseException(MessageId.ExpressionNotIndexable, $"Cannot index {right} expression", peek);
                }

                node.Target = left;
                left        = node;
            }

            peek = _tokens.PeekToken();
        }

        if (peek == null)
        {
            throw new CancelParsingException("Premature EOF");
        }

        while (peek.Precedence > minimumPrecedence)
        {
            Token? op = _tokens.GrabToken();
            if (op == null)
            {
                throw new CancelParsingException("Premature EOF");
            }
            ExpressionNode right = ExpectExpression(scope);

            peek = _tokens.PeekToken();
            if (peek == null)
            {
                return left;
            }

            while (peek.Precedence > op.Precedence
                   || peek.IsRightAssocBinOp && peek.Precedence == op.Precedence)
            {
                right = ExpectExpressionRhs(scope, right, op.Precedence + (peek.Precedence > op.Precedence ? 0 : 1));
            }

            left = new BinaryOperatorNode(op.Type, left, right) { Location = op.Location };
        }

        return left;
    }

    /// <summary>
    /// Expects a new class instance (e.g. new app::Class())
    /// </summary>
    private InitializerNode ExpectNew(SymbolTable scope)
    {
        Token newKeyword = _tokens.GrabToken()!;

        TypeReferenceNode type = ExpectTypeName(scope);

        if (_tokens.NextIsOfType(TokenType.LeftParen))
        {
            // we have a class initializer

            ExpressionNode[] constructorArgs = GrabTupleValues(scope);

            return new NewClassInitializerNode(type, constructorArgs) { Location = type.Location };
        }
        
        if (!_tokens.HasNext)
        {
            throw new CancelParsingException("Premature EOF");
        }
        
        throw new ParseException(
            MessageId.UnexpectedToken,
            $"Expected [ or ( after new, got {_tokens.PeekToken()!.Type}",
            _tokens.PeekToken()!
        );
    }

    /// <summary>
    /// Utility method for expecting a value list.
    /// </summary>
    private ExpressionNode[] GrabTupleValues(SymbolTable scope)
    {
        Require(TokenType.LeftParen);
        List<ExpressionNode> expressions = new();

        while (!_tokens.NextIsOfType(TokenType.RightParen))
        {
            expressions.Add(ExpectExpression(scope));

            Token? next = _tokens.PeekToken();

            if (next == null)
            {
                throw new CancelParsingException("Premature EOF");
            }

            if (next.Type == TokenType.RightParen)
            {
                break;
            }

            if (next.Type == TokenType.Comma)
            {
                _tokens.Skip();
            }
            else
            {
                MessageCollection.Error(MessageId.UnexpectedToken, $"Expected comma or ), got {next}", next.Location);
            }
        }

        Require(TokenType.RightParen);

        return expressions.ToArray();
    }

    /// <summary>
    /// Utility method for expecting an argument list.
    /// </summary>
    private VariableNode[] GrabTupleTypeDefinition(SymbolTable scope)
    {
        List<VariableNode> vars = new();

        Require(TokenType.LeftParen);
        while (!_tokens.NextIsOfType(TokenType.RightParen))
        {
            TypeReferenceNode type = ExpectTypeName(scope);
            string name = GrabNextByExpecting(TokenType.Identifier);
            Token nameToken = _tokens.CurrentToken;
            vars.Add(
                new VariableNode(
                    type,
                    name,
                    null
                ) { Location = nameToken.Location }
            );

            Token? peek = _tokens.PeekToken();

            if (peek == null)
            {
                throw new CancelParsingException("Premature EOF while parsing argument list");
            }

            if (peek.Type == TokenType.Comma)
            {
                _tokens.Skip(); // ,
            }
            else if (peek.Type != TokenType.RightParen)
            {
                throw new ParseException(MessageId.UnexpectedToken, $"Expected comma or closing paren, got {peek}", peek);
            }
        }
        Require(TokenType.RightParen);

        return vars.ToArray();
    }

    /// <summary>
    /// Utility method for parsing using directives.
    /// </summary>
    private IEnumerable<(string, TokenLocation)> GrabUsingDirectives()
    {
        List<(string, TokenLocation)> dependencies = new();

        while (_tokens.NextIsOfType(TokenType.Using))
        {
            _tokens.Skip(); // using

            try
            {
                TokenLocation? location = _tokens.PeekToken()?.Location;

                if (location == default)
                {
                    throw new CancelParsingException("Premature EOF");
                }

                // This ! is required!!! The program will sometimes not compile without it.
                // ReSharper disable once RedundantSuppressNullableWarningExpression
                dependencies.Add((GrabModuleName(), location!));
            }
            catch (ParseException ex)
            {
                LogErrorAndRecover(ex, false);
            }

            Require(TokenType.Semicolon);
        }

        return dependencies.ToArray();
    }

    /// <summary>
    /// Utility method for getting a list of modifiers.
    /// </summary>
    private string[] GrabModifiers()
    {
        List<string> modifiers = new();

        while (_tokens.NextIsOfType(TokenType.Modifier))
        {
            modifiers.Add(_tokens.GrabToken()!.Value);
        }

        return modifiers.ToArray();
    }

    /// <summary>
    /// Grabs the next token and returns its value.
    /// Logs an error if it isn't of the expected type.
    /// </summary>
    /// <param name="expected">The expected type token.</param>
    /// <returns>The value of the next token.</returns>
    /// <exception cref="ParseException">On EOF</exception>
    private string GrabNextByExpecting(TokenType expected)
    {
        Token? token = _tokens.GrabToken();

        if (token == null)
        {
            throw new CancelParsingException($"Expected {expected}, but file ended");
        }

        if (token.Type != expected)
        {
            MessageCollection.Error(MessageId.UnexpectedToken, $"Expected {expected}, got {token.Type}", token.Location);
        }

        return token.Value;
    }

    /// <summary>
    /// Utility method for parsing the module name directive.
    /// </summary>
    /// <returns>The name of the module.</returns>
    private string GrabModuleDirective()
    {
        string name;
        if (_tokens.TryExpect(TokenType.Module, out Token next))
        {
            _tokens.Skip(); // module

            try
            {
                name = GrabModuleName();
                Require(TokenType.Semicolon);
            }
            catch (ParseException ex)
            {
                name = "<error_in_module_name>";
                LogErrorAndRecover(ex, true);
            }
        }
        else
        {
            name = "<no_name_given>";
            MessageCollection.Error(
                MessageId.UnexpectedToken,
                $"Expected module name statement, got {next.Type.ToString()}",
                next.Location
            );
        }

        return name;
    }

    /// <summary>
    /// Utility method for reading a module name.
    /// </summary>
    /// <returns>The module name as a string</returns>
    /// <exception cref="ParseException">Thrown for invalid tokens after ::</exception>
    private string GrabModuleName()
    {
        if (!_tokens.TryExpect(TokenType.Identifier, out Token? fail))
        {
            MessageCollection.Error(MessageId.UnexpectedToken, "Expected module name", fail.Location);
        }

        StringBuilder name = new();

        while (_tokens.TryExpect(TokenType.Identifier, out Token current))
        {
            _tokens.Skip(); // current token
            name.Append(current.Value);

            if (_tokens.NextIsOfType(TokenType.DoubleColon))
            {
                name.Append(_tokens.GrabToken()!.Value); // add separator

                if (!_tokens.TryExpect(TokenType.Identifier, out Token? offending))
                {
                    throw new ParseException(
                        MessageId.UnexpectedToken,
                        $"Expected identifier in module name, got {offending.Type}",
                        offending
                    );
                }
            }
            else
            {
                break;
            }
        }
        return name.ToString();
    }

    /// <summary>
    /// Utility method for reading a type name.
    /// </summary>
    /// <param name="scope">Used for later lookups (during semantic analysis), so the type can be found.</param>
    private TypeReferenceNode ExpectTypeName(SymbolTable scope)
    {
        // In all cases we need an identifier
        // If the token after it is a double colon, we get module name first

        // UNLESS!
        // It's void :sunglasses:
        if (_tokens.PeekToken()?.Type == TokenType.Void)
        {
            return new VoidTypeReferenceNode
            {
                Location = _tokens.GrabToken()!.Location
            };
        }

        string name = GrabModuleName();
        Token nameToken = _tokens.CurrentToken;

        if (!_tokens.NextIsOfType(TokenType.LeftAngle))
        {
            bool nullable = _tokens.NextIsOfType(TokenType.Question);
            if (nullable)
            {
                _tokens.Skip(); // ?
            }

            TypeReferenceNode node = new(name, nullable, Array.Empty<TypeReferenceNode>())
            {
                Location = nameToken.Location
            };
            node.SetMetadata(scope);
            return node;
        }
        else
        {
            // We have some generic args
            List<TypeReferenceNode> genericNames = new();

            do
            {
                _tokens.Skip(); // < or comma!!
                genericNames.Add(ExpectTypeName(scope));
            }
            while (_tokens.NextIsOfType(TokenType.Comma));

            Require(TokenType.RightAngle);

            bool nullable = _tokens.NextIsOfType(TokenType.Question);
            if (nullable)
            {
                _tokens.Skip(); // ?
            }

            TypeReferenceNode node = new(name, nullable, genericNames.ToArray()) { Location = nameToken.Location };
            node.SetMetadata(scope);
            return node;
        }
    }

    /// <summary>
    /// Understands what the visibility of a symbols is by looking at modifiers.
    /// </summary>
    private GetAccessibility VisibilityFromModifiers(string[] modifiers, MessageId codeStyleWarningCode)
    {
        if (modifiers.Contains("public"))
        {
            return GetAccessibility.Public;
        }
        if (modifiers.Contains("protected"))
        {
            return GetAccessibility.Protected;
        }
        if (modifiers.Contains("private"))
        {
            return GetAccessibility.Private;
        }

        if (!modifiers.Contains("internal"))
        {
            MessageCollection.Warn(
                codeStyleWarningCode,
                "Always specify visibility. Using internal.",
                _tokens.CurrentToken.Location
            );
        }

        return GetAccessibility.Internal;
    }

    /// <summary>
    /// Utility method for requiring a token.
    /// </summary>
    private void Require(TokenType expected)
    {
        if (_tokens.TryExpect(expected, out Token? fail))
        {
            _tokens.Skip();
        }
        else
        {
            if (fail == null)
            {
                throw new CancelParsingException("Premature EOF");
            }

            throw new ParseException(MessageId.UnexpectedToken, $"Expected {expected}, got {fail.Type.ToString()}", fail);
        }
    }

    /// <summary>
    /// Adds an error for every modifier that's not in the allowed modifiers.
    /// </summary>
    /// <param name="modifiers">Modifiers to check.</param>
    /// <param name="errorToken">The token to use for error reporting.</param>
    /// <param name="allowed">Allowed modifiers.</param>
    private void ApplyModifierFilters(IEnumerable<string> modifiers, Token errorToken, params string[] allowed)
    {
        foreach (string mod in modifiers)
        {
            if (!allowed.Contains(mod))
            {
                MessageCollection.Error(MessageId.InapplicableModifier, $"Invalid modifier {mod}", errorToken.Location);
            }
        }
    }

    /// <summary>
    /// Creates a new parser at the same position and file path. This is used to test
    /// possible ahead syntax without needing to backtrack.
    /// </summary>
    /// <remarks>Creating a new parser has its overhead. Use with caution.</remarks>
    private Parser CreateSubParser()
    {
        return new Parser(new Tokens(_tokens), _path);
    }

    /// <summary>
    /// Utility method for logging parse exceptions.
    /// Recovers from errors by going to the next semicolon or right brace.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="recoverConsumes">Whether or not the recover process should consume the final token.</param>
    /// <exception cref="CancelParsingException">When there are too many parse errors</exception>
    private void LogErrorAndRecover(ParseException ex, bool recoverConsumes)
        => LogErrorAndRecover(ex.MessageId, ex.Message, ex.OffendingToken.Location, recoverConsumes);

    /// <summary>
    /// Utility method for logging parse exceptions.
    /// Recovers from errors by going to the next semicolon or right brace.
    /// </summary>
    /// <param name="id">The ID of the message.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="location">The location of the offending code.</param>
    /// <param name="recoverConsumes">Whether or not the recover process should consume the final token.</param>
    /// <exception cref="CancelParsingException">When there are too many parse errors</exception>
    private void LogErrorAndRecover(MessageId id, string message, TokenLocation location, bool recoverConsumes)
    {
        MessageCollection.Error(id, message, location);

        if (MessageCollection.Count(x => x.Fatality == MessageFatality.Severe) > 8)
        {
            throw new CancelParsingException("Too many errors found in file during parsing");
        }

        // Recover
        while (!_tokens.NextIsOfType(TokenType.RightBrace)
               && !_tokens.TryExpect(TokenType.Semicolon, out Token _))
        {
            _tokens.Skip();
        }

        if (recoverConsumes)
        {
            _tokens.Skip(); // we don't actually skip the semicolon/brace itself in the loop
        }
    }
}