using System.Text;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Frontend.Lexing;
using static MarlinCompiler.Frontend.Lexing.Lexer;

namespace MarlinCompiler.Frontend;

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
        MessageCollection = new MessageCollection();
        _tokens = tokens;
        _path = filePath;
        _moduleName = "<global>";
        _compilationUnitDependencies = new List<(string, FileLocation)>();
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
    private readonly List<(string, FileLocation)> _compilationUnitDependencies;
    
    /// <summary>
    /// An exception for parse errors.
    /// </summary>
    private class ParseException : Exception
    {
        /// <summary>
        /// The token that caused the error.
        /// </summary>
        public Token OffendingToken { get; }

        public ParseException(string errorMessage, Token offendingToken) : base(errorMessage)
        {
            OffendingToken = offendingToken;
        }
    }

    /// <summary>
    /// Exception used to cancel the parsing process due to too many errors.
    /// </summary>
    private class CancelParsingException : Exception
    {
        public CancelParsingException(string reason) : base(reason)
        {
        }
    }

    /// <summary>
    /// Starts the parse operation.
    /// </summary>
    /// <returns>The compilation unit node or null when the token source is empty.</returns>
    public CompilationUnitNode Parse()
    {
        if (!_tokens.HasNext) return null!;

        return ExpectCompilationUnit();
    }
    
    /// <summary>
    /// Tries to parse a compilation unit. Does not throw parse exceptions.
    /// </summary>
    private CompilationUnitNode ExpectCompilationUnit()
    {
        _compilationUnitDependencies.AddRange(GrabUsingDirectives());
        string name = GrabModuleDirective();
        _moduleName = name;

        List<Node> children = new();
        
        try
        {
            while (_tokens.HasNext)
            {
                try
                {
                    Node? child = ExpectTypeDefinition();

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
            MessageCollection.Info($"Parsing cancelled: {ex.Message}", new FileLocation(_path));
        }

        CompilationUnitNode node = new(name, _compilationUnitDependencies.ToArray());
        node.Children.AddRange(children);
        return node;
    }

    /// <summary>
    /// Expects a type definition.
    /// </summary>
    /// <returns>Null for EOF, otherwise the node of the type def.</returns>
    private TypeDefinitionNode? ExpectTypeDefinition()
    {
        Token? next = _tokens.PeekNextTokenBySkipping(TokenType.Modifier, 0);

        if (next == null)
        {
            return null;
        }

        return next.Type switch
        {
            TokenType.Class => ExpectClassDefinition(),
            TokenType.Struct => ExpectStructDefinition(),
            TokenType.Extern => ExpectExternTypeDefinition(),
            _ => throw new ParseException($"Expected type definition, got {next.Type}", next)
        };
    }

    /// <summary>
    /// Expects a type body, including the braces.
    /// </summary>
    private ContainerNode ExpectTypeBody()
    {
        ContainerNode bodyNode = new();
        
        Require(TokenType.LeftBrace);
        while (!_tokens.NextIsOfType(TokenType.RightBrace))
        {
            try
            {
                Node child = ExpectTypeMember();
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
    /// Expects a body for an externed type, including the braces.
    /// An externed body consists of method mappings.
    /// </summary>
    private ContainerNode ExpectExternTypeBody()
    {
        ContainerNode bodyNode = new();
        
        Require(TokenType.LeftBrace);
        while (!_tokens.NextIsOfType(TokenType.RightBrace))
        {
            try
            {
                Node child = ExpectExternMethod();

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
    private Node ExpectTypeMember()
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
            return ExpectConstructor();
        }
        
        tempParser.ExpectTypeName(); // don't care about this either
        tempParser.GrabNextByExpecting(TokenType.Identifier); // nope
        
        // and now we can just check if the next token is a left paren
        if (tempParser._tokens.NextIsOfType(TokenType.LeftParen))
        {
            // Method!
            return ExpectMethodDeclaration();
        }
        else
        {
            // Property! (probably, it'll handle validation itself)
            return ExpectPropertyDeclaration();
        }
    }

    /// <summary>
    /// Expects an externed method or constructor mapping.
    /// </summary>
    private Node ExpectExternMethod()
    {
        string[] modifiers = GrabModifiers();
        TypeReferenceNode type;

        string? name = null;

        if (_tokens.NextIsOfType(TokenType.Constructor))
        {
            type = new TypeReferenceNode("<$constructor$>");
            _tokens.Skip(); // constructor
        }
        else
        {
            type = ExpectTypeName();
            name = GrabNextByExpecting(TokenType.Identifier);
        }

        Token nameToken = _tokens.CurrentToken;
        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "protected", "private", "static");
        VariableNode[] expectedArgs = GrabTupleTypeDefinition();

        GrabNextByExpecting(TokenType.Arrow);
        GrabNextByExpecting(TokenType.At);

        GrabNextByExpecting(TokenType.Identifier);
        ExpressionNode[] passedArgs = GrabTupleValues();

        Require(TokenType.Semicolon);

        GetAccessibility getAccessibility = VisibilityFromModifiers(modifiers);
        bool isStatic = modifiers.Contains("static");
        return new ExternedMethodNode(
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
    }
    
    /// <summary>
    /// Expects a class definition.
    /// </summary>
    private ClassTypeDefinitionNode ExpectClassDefinition()
    {
        string[] modifiers = GrabModifiers();
        
        GrabNextByExpecting(TokenType.Class);
        
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        GetAccessibility accessibility = VisibilityFromModifiers(modifiers);
        
        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "static");

        string? genericTypeParamName = null;
        if (_tokens.NextIsOfType(TokenType.LeftAngle))
        {
            _tokens.Skip();
            genericTypeParamName = GrabNextByExpecting(TokenType.Identifier);
            Require(TokenType.RightAngle);
        }
        
        TypeReferenceNode? baseClass = null;
        if (_tokens.NextIsOfType(TokenType.Colon))
        {
            _tokens.Skip(); // colon
            baseClass = ExpectTypeName();
        }
        else if (_moduleName + "::Object" != "std::Object") // don't make the base obj inherit from itself lol
        {
            baseClass = new TypeReferenceNode("std::Object") { Location = nameToken.Location };
        }

        bool isStatic = modifiers.Contains("static");
        ClassTypeDefinitionNode classNode = new(
            name,
            _moduleName,
            accessibility,
            isStatic,
            baseClass,
            genericTypeParamName
        )
        {
            Location = nameToken.Location,
        };

        // Add type body
        ContainerNode typeBody = ExpectTypeBody();
        classNode.Children.AddRange(typeBody);
        
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
        GetAccessibility accessibility = VisibilityFromModifiers(modifiers);
        
        ApplyModifierFilters(modifiers, nameToken, "public", "internal");

        StructTypeDefinitionNode structNode = new(
            name,
            _moduleName,
            accessibility
        )
        {
            Location = nameToken.Location
        };
        
        // Type body
        ContainerNode typeBody = ExpectTypeBody();
        structNode.Children.AddRange(typeBody);
        
        return structNode;
    }

    /// <summary>
    /// Expects an external struct mapping.
    /// </summary>
    private ExternedTypeDefinitionNode ExpectExternTypeDefinition()
    {
        string[] modifiers = GrabModifiers();

        GrabNextByExpecting(TokenType.Extern);

        string name = GrabNextByExpecting(TokenType.Identifier);
        string? llvmTypeName = null;
        Token nameToken = _tokens.CurrentToken;
        GetAccessibility accessibility = VisibilityFromModifiers(modifiers);
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
                    $"Expected LLVM type name for non-static externed type {name}",
                    nameToken.Location
                );
            }
        }

        ExternedTypeDefinitionNode node = new(name, _moduleName, accessibility, isStatic, llvmTypeName)
        {
            Location = nameToken.Location
        };

        // Type body
        ContainerNode body = ExpectExternTypeBody();
        node.Children.AddRange(body.Children);

        return node;
    }
    
    /// <summary>
    /// Expects a constructor declaration.
    /// </summary>
    private ConstructorDeclarationNode ExpectConstructor()
    {
        string[] modifiers = GrabModifiers();
        GrabNextByExpecting(TokenType.Constructor);
        Token ctorToken = _tokens.CurrentToken;
        
        ApplyModifierFilters(modifiers, ctorToken, "public", "internal", "protected", "private");
        VariableNode[] args = GrabTupleTypeDefinition();

        GetAccessibility accessibility = VisibilityFromModifiers(modifiers);
        
        ConstructorDeclarationNode node = new(
            accessibility,
            args
        )
        {
            Location = ctorToken.Location
        };

        // Body!!!
        ContainerNode body = ExpectStatementBody(false);
        node.Children.AddRange(body);

        return node;
    }

    /// <summary>
    /// Expects a method declaration. 
    /// </summary>
    private MethodDeclarationNode ExpectMethodDeclaration()
    {
        string[] modifiers = GrabModifiers();
        TypeReferenceNode type = ExpectTypeName();
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "protected", "private", "static");
        VariableNode[] args = GrabTupleTypeDefinition();

        GetAccessibility accessibility = VisibilityFromModifiers(modifiers);
        bool isStatic = modifiers.Contains("static");
        
        MethodDeclarationNode node = new(
            accessibility,
            type,
            name,
            isStatic,
            args
        )
        {
            Location = nameToken.Location
        };

        // Body!!!!
        ContainerNode body = ExpectStatementBody(false);
        node.Children.AddRange(body.Children);

        return node;
    }

    /// <summary>
    /// Expects a collection of statements surrounded by curly braces.
    /// </summary>
    /// <param name="insideLoop">Are we inside a loop? This enables continue and break statements.</param>
    /// TODO Implement insideLoop
    private ContainerNode ExpectStatementBody(bool insideLoop)
    {
        ContainerNode node = new();
        
        Require(TokenType.LeftBrace);
        while (!_tokens.NextIsOfType(TokenType.RightBrace))
        {
            try
            {
                Node child = ExpectStatement(insideLoop);

                node.Children.Add(child);
            }
            catch (ParseException ex)
            {
                LogErrorAndRecover(ex, false);
            }
        }

        Require(TokenType.RightBrace);

        return node;
    }

    /// <summary>
    /// Expects a property declaration.
    /// </summary>
    private Node ExpectPropertyDeclaration()
    {
        string[] modifiers = GrabModifiers();
        TypeReferenceNode type = ExpectTypeName();
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        
        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "protected", "private", "static");
        GetAccessibility get = VisibilityFromModifiers(modifiers);
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
                if (peek == null) throw new CancelParsingException("Premature EOF");
                
                string currentAccessibility = get.ToString().ToLower();
                
                if (peek.Type == TokenType.Modifier)
                {
                    currentAccessibility = _tokens.GrabToken()!.Value;

                    if (currentAccessibility is not ("public" or "private" or "internal"))
                    {
                        MessageCollection.Error(
                            $"Unknown visibility specifier {currentAccessibility}",
                            peek.Location
                        );
                        currentAccessibility = "internal";
                    }

                    peek = _tokens.PeekToken();
                    if (peek == null) throw new CancelParsingException("Premature EOF");
                }

                if (peek.Type == TokenType.Get)
                {
                    if (assignedGet)
                    {
                        MessageCollection.Error("Repeated get specifier", peek.Location);
                    }
                    assignedGet = true;

                    if (currentAccessibility.ToLower() != get.ToString().ToLower())
                    {
                        MessageCollection.Error(
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
                        MessageCollection.Error("Repeated set specifier", peek.Location);
                    }
                    assignedSet = true;
                    Enum.TryParse(currentAccessibility, true, out set);

                    if ((short)set > (short)get)
                    {
                        MessageCollection.Error(
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
                    throw new ParseException("Expected specifier", peek);
                }
            }

            if (!assignedSet)
            {
                set = SetAccessibility.NoModify;
            }
            else if (!assignedGet && assignedSet)
            {
                MessageCollection.Error(
                    "Cannot have only set specifier (you must add get as well)",
                    nameToken.Location
                );
            }
            
            if (assignedGet && !assignedSet)
            {
                MessageCollection.Warn(
                    "Don't add redundant get specifiers (missing set specifier means readonly anyway)",
                    nameToken.Location
                );
            }
        }
        
        if (_tokens.NextIsOfType(TokenType.Assign))
        {
            _tokens.Skip(); // =
            value = ExpectExpression();
        }
        
        Require(TokenType.Semicolon);

        bool isStatic = modifiers.Contains("static");
        return new PropertyNode(
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
    }

    /// <summary>
    /// Expects a local variable declaration.
    /// </summary>
    private VariableNode ExpectLocalVariableDeclaration()
    {
        bool mutable = _tokens.NextIsOfType(TokenType.Mutable);
        if (mutable)
        {
            _tokens.Skip(); // mut
        }
        
        TypeReferenceNode type = ExpectTypeName();
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        ExpressionNode? value = null;

        if (_tokens.NextIsOfType(TokenType.Assign))
        {
            _tokens.Skip(); // =
            value = ExpectExpression();
        }
        
        Require(TokenType.Semicolon);

        return new LocalVariableDeclarationNode(
            type,
            name,
            mutable,
            value
        )
        {
            Location = nameToken.Location
        };
    }

    /// <summary>
    /// Expects any statement.
    /// </summary>
    /// <param name="insideLoop">Is this a statement inside a loop? This enables continue and break statements.</param>
    private Node ExpectStatement(bool insideLoop)
    {
        // Statements:
        //   statement block
        //   local variable declaration         type identifier (assign expr)? semicolon
        //   variable assignment                (accessPath dot)? identifier assign expr semicolon
        //   method call                        (accessPath dot)? identifier tuple semicolon
        //   todo more

        // Empty statement
        if (_tokens.NextIsOfType(TokenType.Semicolon))
        {
            return new EmptyStatementNode()
            {
                Location = _tokens.GrabToken()!.Location
            };
        }
        
        // Statement block
        if (_tokens.NextIsOfType(TokenType.LeftBrace))
        {
            return ExpectStatementBody(insideLoop);
        }
        
        // Variable declaration
        try
        {
            Parser parser = CreateSubParser();

            if (parser._tokens.NextIsOfType(TokenType.Mutable))
            {
                parser._tokens.Skip();
            }
            
            parser.ExpectTypeName(); // var type
            parser.GrabNextByExpecting(TokenType.Identifier);
            if (parser.MessageCollection.HasFatalErrors) throw new FormatException();

            Token? next = parser._tokens.GrabToken();
            if (next == null) throw new CancelParsingException("Premature EOF");
            if (next.Type == TokenType.Semicolon || next.Type == TokenType.Assign)
            {
                return ExpectLocalVariableDeclaration(); // use regular parser, not subparser!
            }
        }
        catch (FormatException) {}
        
        // Try getting an expression chain (method call/assignment)
        try
        {
            CreateSubParser();

            ExpressionNode expr = ExpectExpression(); // should work if the subparser didn't error

            if (expr is MethodCallNode || expr is VariableAssignmentNode)
            {
                Require(TokenType.Semicolon);
                return expr;
            }
        }
        catch (ParseException) {}
        
        // Cannot figure out what this is
        throw new ParseException(
            $"Expected statement, got {_tokens.PeekToken()!.Type}",
            _tokens.GrabToken()!
        );
    }

    /// <summary>
    /// Expects a variable assignment.
    /// </summary>
    private VariableAssignmentNode ExpectVariableAssignment()
    {
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        GrabNextByExpecting(TokenType.Assign);
        ExpressionNode value = ExpectExpression(); 
        return new VariableAssignmentNode(name, value) { Location = nameToken.Location };
    }

    /// <summary>
    /// Expects a method call.
    /// </summary>
    private MethodCallNode ExpectMethodCall()
    {
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        ExpressionNode[] args = GrabTupleValues();

        bool isNativeCall = _tokens.NextIsOfType(TokenType.At);
        return new MethodCallNode(name, isNativeCall, args)
        {
            Location = nameToken.Location
        };
    }

    /// <summary>
    /// Expects any expression.
    /// </summary>
    private ExpressionNode ExpectExpression(bool doNotEvaluateOperators = false)
    {
        Token? next = _tokens.PeekToken();
        if (next == null) throw new CancelParsingException("Expected expression, got EOF");

        ExpressionNode expr;
        
        switch (next.Type)
        {
            case TokenType.LeftParen:
                Require(TokenType.LeftParen);
                expr = ExpectExpression();
                Require(TokenType.RightParen);
                break;

            case TokenType.Integer:
                expr = new IntegerNode(Int32.Parse(_tokens.GrabToken()!.Value)) { Location = next.Location };
                break;
                
            case TokenType.Decimal:
                throw new NotImplementedException();
            
            case TokenType.New:
                expr = ExpectNew();
                break;
            
            case TokenType.Identifier:
                Token? peek = _tokens.PeekToken(2);
                if (peek == null) throw new CancelParsingException("Expected expression, got EOF");

                if (peek.Type == TokenType.DoubleColon)
                {
                    expr = ExpectTypeName();
                }
                else if (peek.Type == TokenType.LeftParen)
                {
                    expr = ExpectMethodCall();
                }
                else if (peek.Type == TokenType.Assign)
                {
                    expr = ExpectVariableAssignment();
                }
                else
                {
                    expr = new MemberAccessNode(_tokens.GrabToken()!.Value) { Location = peek.Location };
                }
                
                break;

            default:
                throw new ParseException($"Expected expression, got {next.Type}", next);
        }

        if (doNotEvaluateOperators) return expr;
        return ExpectExpressionRhs(expr, 0);
    }

    /// <summary>
    /// Complements ExpectExpression. Takes care of operation precedence.
    /// </summary>
    /// <remarks>https://en.wikipedia.org/wiki/Operator-precedence_parser#Pseudocode</remarks>
    private ExpressionNode ExpectExpressionRhs(ExpressionNode left, int minimumPrecedence)
    {
        Token? peek = _tokens.PeekToken();
        if (peek == null) throw new CancelParsingException("Premature EOF");

        // MEMBER ACCESS
        if (peek.Type == TokenType.Dot)
        {
            while (_tokens.NextIsOfType(TokenType.Dot))
            {
                peek = _tokens.GrabToken()!; // .
                ExpressionNode right = ExpectExpression(true);

                if (right is not IndexableExpressionNode node)
                {
                    throw new ParseException($"Cannot index {right} expression", peek);
                }

                node.Target = left;
                left = node;
            }

            peek = _tokens.PeekToken();
        }

        if (peek == null) throw new CancelParsingException("Premature EOF");

        while (peek.Precedence > minimumPrecedence)
        {
            Token? op = _tokens.GrabToken();
            if (op == null) throw new CancelParsingException("Premature EOF");
            ExpressionNode right = ExpectExpression();

            peek = _tokens.PeekToken();
            if (peek == null) return left;
            
            while (peek.Precedence > op.Precedence
                   || (peek.IsRightAssocBinOp && peek.Precedence == op.Precedence))
            {
                right = ExpectExpressionRhs(right, op.Precedence + (peek.Precedence > op.Precedence ? 0 : 1));
            }

            left = new BinaryOperatorNode(op.Type, left, right) { Location = op.Location };
        }

        return left;
    }

    /// <summary>
    /// Expects a new class instance (e.g. new app::Class())
    /// </summary>
    private InitializerNode ExpectNew()
    {
        Token newKeyword = _tokens.GrabToken()!;

        TypeReferenceNode type = ExpectTypeName();

        if (_tokens.NextIsOfType(TokenType.LeftParen))
        {
            // we have a class initializer

            ExpressionNode[] constructorArgs = GrabTupleValues();

            return new NewClassInitializerNode(type, constructorArgs) { Location = newKeyword.Location };
        }
        else if (!_tokens.HasNext)
        {
            throw new CancelParsingException("Premature EOF");
        }
        else
        {
            throw new ParseException(
                $"Expected [ or ( after new, got {_tokens.PeekToken()!.Type}",
                _tokens.PeekToken()!
            );
        }
    }

    /// <summary>
    /// Utility method for expecting a value list.
    /// </summary>
    private ExpressionNode[] GrabTupleValues()
    {
        Require(TokenType.LeftParen);
        List<ExpressionNode> expressions = new();
        
        while (!_tokens.NextIsOfType(TokenType.RightParen))
        {
            expressions.Add(ExpectExpression());

            Token? next = _tokens.PeekToken();

            if (next == null) throw new CancelParsingException("Premature EOF");
            
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
                MessageCollection.Error($"Expected comma or ), got {next}", next.Location);
            }
        }
        
        Require(TokenType.RightParen);

        return expressions.ToArray();
    }

    /// <summary>
    /// Utility method for expecting an argument list.
    /// </summary>
    private VariableNode[] GrabTupleTypeDefinition()
    {
        List<VariableNode> vars = new();

        Require(TokenType.LeftParen);
        while (!_tokens.NextIsOfType(TokenType.RightParen))
        {
            TypeReferenceNode type = ExpectTypeName();
            string name = GrabNextByExpecting(TokenType.Identifier);
            Token nameToken = _tokens.CurrentToken;
            vars.Add(new VariableNode(
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
                throw new ParseException($"Expected comma or closing paren, got {peek}", peek);
            }
        }
        Require(TokenType.RightParen);

        return vars.ToArray();
    }
    
    /// <summary>
    /// Utility method for parsing using directives.
    /// </summary>
    private IEnumerable<(string, FileLocation)> GrabUsingDirectives()
    {
        List<(string, FileLocation)> dependencies = new();
        
        while (_tokens.NextIsOfType(TokenType.Using))
        {
            _tokens.Skip(); // using

            try
            {
                FileLocation? location = _tokens.PeekToken()?.Location;

                if (location == default)
                {
                    throw new CancelParsingException("Premature EOF");
                }

                dependencies.Add((GrabModuleName(), (FileLocation) location!));
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
            MessageCollection.Error($"Expected {expected}, got {token.Type}", token.Location);
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
            MessageCollection.Error("Expected module name", fail.Location);
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
    private TypeReferenceNode ExpectTypeName()
    {
        // In all cases we need an identifier
        // If the token after it is a double colon, we get module name first

        // UNLESS!
        // It's void :sunglasses:
        if (_tokens.PeekToken()?.Type == TokenType.Void)
        {
            return new VoidTypeReferenceNode()
            {
                Location = _tokens.GrabToken()!.Location
            };
        }
        
        string name = GrabModuleName();
        Token nameToken = _tokens.CurrentToken;
        
        TypeReferenceNode? genericName = null;

        if (!_tokens.NextIsOfType(TokenType.LeftAngle))
            return new TypeReferenceNode(name, genericName) {Location = nameToken.Location};
        
        _tokens.Skip(); // <

        genericName = ExpectTypeName();
            
        Require(TokenType.RightAngle);

        return new TypeReferenceNode(name, genericName) { Location = nameToken.Location };
    }

    /// <summary>
    /// Understands what the visibility of a symbols is by looking at modifiers.
    /// </summary>
    private GetAccessibility VisibilityFromModifiers(string[] modifiers)
    {
        if (modifiers.Contains("public"))
            return GetAccessibility.Public;
        if (modifiers.Contains("protected"))
            return GetAccessibility.Protected;
        if (modifiers.Contains("private"))
            return GetAccessibility.Private;

        if (!modifiers.Contains("internal"))
        {
            MessageCollection.Warn(
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
            
            throw new ParseException($"Expected {expected}, got {fail.Type.ToString()}", fail);
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
                MessageCollection.Error($"Invalid modifier {mod}", errorToken.Location);
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
    {
        MessageCollection.Error(ex.Message, ex.OffendingToken.Location);

        if (MessageCollection.Count(x => x.Fatality == MessageFatality.Severe) > 8)
        {
            throw new CancelParsingException("Too many errors found in file during parsing");
        }
        
        // Recover
        while (!_tokens.NextIsOfType(TokenType.RightBrace) && !_tokens.TryExpect(TokenType.Semicolon, out Token _))
        {
            _tokens.Skip();
        }

        if (recoverConsumes)
        {
            _tokens.Skip(); // we don't actually skip the semicolon/brace itself in the loop
        }
    }
}