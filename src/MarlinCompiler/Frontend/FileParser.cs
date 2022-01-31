using System.Text;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using static MarlinCompiler.Frontend.Lexer;

namespace MarlinCompiler.Frontend;

/// <summary>
/// Parser class.
/// Naming conventions:
/// - Expect... - method for getting whole nodes
/// - Grab... - method for getting non-nodes, e.g. a type name
/// - Require... - method that adds an error if something is missing (e.g. semicolon)
/// </summary>
public sealed class FileParser
{
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
    /// Constructor.
    /// </summary>
    /// <param name="filePath">Path to the source file. Used solely for error reporting.</param>
    public FileParser(Tokens tokens, string filePath)
    {
        MessageCollection = new MessageCollection();
        _tokens = tokens;
        _path = filePath;
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
        string[] dependencies = GrabUsingDirectives();
        string name = GrabModuleDirective();

        CompilationUnitNode node = new(name, dependencies);

        try
        {
            while (_tokens.HasNext)
            {
                try
                {
                    Node? child = ExpectTypeDefinition();

                    if (child != null)
                    {
                        node.Children.Add(child);
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
            MessageCollection.Info($"Parsing of {_path} cancelled: {ex.Message}");
        }

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
            _ => throw new ParseException($"Expected type definition, got {next.Type}", next)
        };
    }

    /// <summary>
    /// Expects a type body, including the braces.
    /// </summary>
    private ContainerNode ExpectTypeBody()
    {
        ContainerNode bodyNode = new();
        
        GrabNextByExpecting(TokenType.LeftBrace);
        while (!_tokens.NextIsOfType(TokenType.RightBrace))
        {
            try
            {
                Node child = ExpectTypeMember();
                if (child != null)
                {
                    bodyNode.Children.Add(child);
                }
            }
            catch (ParseException ex)
            {
                LogErrorAndRecover(ex, true);
            }
        }
        GrabNextByExpecting(TokenType.RightBrace);

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
        FileParser tempParser = CreateSubParser();
        
        // Type members are method declarations and property declarations
        // Both start off with modifiers, type name and then the name of the member
        // but then methods have a left paren, which is what we'll use to determine
        // which one we have
        tempParser.GrabModifiers(); // don't care about those
        tempParser.GrabTypeName(); // don't care about this either
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
    /// Expects a class definition.
    /// </summary>
    private ClassTypeDefinitionNode ExpectClassDefinition()
    {
        string[] modifiers = GrabModifiers();

        GrabNextByExpecting(TokenType.Class);
        
        string name = GrabNextByExpecting(TokenType.Identifier);
        GetAccessibility accessibility = VisibilityFromModifiers(modifiers);
        
        string? baseClass = null;
        if (_tokens.NextIsOfType(TokenType.Colon))
        {
            _tokens.Skip(); // colon
            baseClass = GrabNextByExpecting(TokenType.Identifier);
        }
        
        ClassTypeDefinitionNode classNode = new ClassTypeDefinitionNode(
            name,
            accessibility,
            baseClass
        );
        
        classNode.Children.AddRange(ExpectTypeBody());
        return classNode;
    }

    /// <summary>
    /// Expects a method declaration. 
    /// </summary>
    private Node ExpectMethodDeclaration()
    {
        string[] modifiers = GrabModifiers();
        string type = GrabTypeName();
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "protected", "private", "static");
        VariableNode[] args = GrabTupleTypeDefinition();

        MethodDeclarationNode node = new MethodDeclarationNode(
            VisibilityFromModifiers(modifiers),
            new TypeReferenceNode(type),
            name,
            modifiers.Contains("static"),
            args
        );

        node.Children.AddRange(ExpectStatementBody(false));

        return node;
    }

    /// <summary>
    /// Expects a collection of statements surrounded by curly braces.
    /// </summary>
    /// <param name="insideLoop">Are we inside a loop? This enables continue and break statements.</param>
    private ContainerNode ExpectStatementBody(bool insideLoop)
    {
        ContainerNode node = new();
        GrabNextByExpecting(TokenType.LeftBrace);
        while (!_tokens.NextIsOfType(TokenType.RightBrace))
        {
            try
            {
                Node child = ExpectStatement(insideLoop);

                if (child != null)
                {
                    node.Children.Add(child);
                }
            }
            catch (ParseException ex)
            {
                LogErrorAndRecover(ex, false);
            }
        }

        GrabNextByExpecting(TokenType.RightBrace);

        return node;
    }

    /// <summary>
    /// Expects a property declaration.
    /// </summary>
    private Node ExpectPropertyDeclaration()
    {
        string[] modifiers = GrabModifiers();
        string type = GrabTypeName();
        string name = GrabNextByExpecting(TokenType.Identifier);
        Token nameToken = _tokens.CurrentToken;
        ApplyModifierFilters(modifiers, nameToken, "public", "internal", "protected", "private");
        GetAccessibility get = VisibilityFromModifiers(modifiers);
        SetAccessibility set;
        SetAccessibility.TryParse(get.ToString(), true, out set);
        Node? value = null;
        
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

                    if (currentAccessibility.ToString().ToLower() != get.ToString().ToLower())
                    {
                        MessageCollection.Error(
                            "Get accessibility must be the same as the accessibility of the property itself.",
                            peek.Location
                        );
                    }
                    
                    GetAccessibility.TryParse(currentAccessibility, true, out get);
                    
                    _tokens.Skip(); // get

                    if (_tokens.NextIsOfType(TokenType.Comma))
                    {
                        _tokens.Skip(); // ,
                        peek = _tokens.PeekToken();
                        continue;
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
                    SetAccessibility.TryParse(currentAccessibility, true, out set);

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
                        continue;
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
        
        RequireSemicolon();

        return new PropertyNode(
            new TypeReferenceNode(type),
            name,
            modifiers.Contains("static"),
            value,
            get,
            set
        );
    }

    /// <summary>
    /// Expects a local variable declaration.
    /// </summary>
    private VariableNode ExpectLocalVariableDeclaration()
    {
        string type = GrabTypeName();
        string name = GrabNextByExpecting(TokenType.Identifier);
        ExpressionNode? value = null;
        
        if (_tokens.NextIsOfType(TokenType.Assign))
        {
            _tokens.Skip(); // =
            value = ExpectExpression();
        }
        
        RequireSemicolon();

        return new LocalVariableDeclaration(new TypeReferenceNode(type), name, value);
    }

    /// <summary>
    /// Expects any statement.
    /// </summary>
    /// <param name="insideLoop">Is this a statement inside a loop? This enables continue and break statements.</param>
    private Node ExpectStatement(bool insideLoop)
    {
        // Statements:
        //   local variable declaration         type identifier (assign expr)? semicolon
        //   variable assignment                (accessPath dot)? identifier assign expr semicolon
        //   method call                        (accessPath dot)? identifier tuple semicolon
        //   todo more

        FileParser parser;
        
        // Try statements
        
        // Variable declaration
        try
        {
            parser = CreateSubParser();
            string type = parser.GrabTypeName(); // var type
            string name = parser.GrabNextByExpecting(TokenType.Identifier);
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
            parser = CreateSubParser();
            parser.ExpectExpression();

            ExpressionNode expr = ExpectExpression(); // should work if the subparser didn't error

            if (expr is MethodCallNode || expr is VariableAssignmentNode)
            {
                RequireSemicolon();
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
        GrabNextByExpecting(TokenType.Assign);
        ExpressionNode value = ExpectExpression(); 
        return new VariableAssignmentNode(name, value);
    }

    /// <summary>
    /// Expects a method call.
    /// </summary>
    private MethodCallNode ExpectMethodCall()
    {
        string name = GrabNextByExpecting(TokenType.Identifier);
        ExpressionNode[] args = GrabTupleValues();
        return new MethodCallNode(name, args);
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
                GrabNextByExpecting(TokenType.LeftParen);
                expr = ExpectExpression();
                GrabNextByExpecting(TokenType.RightParen);
                break;

            case TokenType.Integer:
                expr = new IntegerNode(Int32.Parse(_tokens.GrabToken()!.Value));
                break;
                
            case TokenType.Decimal:
            case TokenType.New:
                throw new NotImplementedException();
            
            case TokenType.Identifier:
                Token? peek = _tokens.PeekToken(2);
                if (peek == null) throw new CancelParsingException("Expected expression, got EOF");

                if (peek.Type == TokenType.LeftParen)
                {
                    expr = ExpectMethodCall();
                }
                else if (peek.Type == TokenType.Assign)
                {
                    expr = ExpectVariableAssignment();
                }
                else
                {
                    // Regular member access
                    _tokens.Skip(); // the id
                    expr = new MemberAccessNode(next.Value); // use the id's value,
                    // not the one of the next token
                }

                break;
            
            default:
                throw new ParseException($"Expected expression, got {next.Type}", next);
        }

        if (doNotEvaluateOperators) return expr;
        return ExpectExpressionRhs(expr, 0);
    }

    private ExpressionNode ExpectExpressionRhs(ExpressionNode left, int minimumPrecedence)
    {
        // it just works
        // https://en.wikipedia.org/wiki/Operator-precedence_parser#Pseudocode

        Token? peek = _tokens.PeekToken();
        if (peek == null) throw new CancelParsingException("Premature EOF");

        // MEMBER ACCESS
        if (peek.Type == TokenType.Dot)
        {
            /*if (left is not IndexableExpressionNode)
            {
                throw new ParseException("Cannot index non-indexable expression", peek);
            }*/
            
            while (_tokens.NextIsOfType(TokenType.Dot))
            {
                peek = _tokens.GrabToken()!; // .
                ExpressionNode right = ExpectExpression(true);

                if (right is not IndexableExpressionNode)
                {
                    throw new ParseException($"Cannot index {right} expression", peek);
                }

                ((IndexableExpressionNode) right).Target = left;
                left = right;
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

            left = new BinaryOperatorNode(op.Type, left, right);
        }

        return left;
    }

    /// <summary>
    /// Utility method for expecting a value list.
    /// </summary>
    private ExpressionNode[] GrabTupleValues()
    {
        GrabNextByExpecting(TokenType.LeftParen);
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
            else if (next.Type == TokenType.Comma)
            {
                continue;
            }
            else
            {
                MessageCollection.Error($"Expected comma or ), got {next}", next.Location);
            }
        }
        
        GrabNextByExpecting(TokenType.RightParen);

        return expressions.ToArray();
    }

    /// <summary>
    /// Utility method for expecting an argument list.
    /// </summary>
    private VariableNode[] GrabTupleTypeDefinition()
    {
        List<VariableNode> vars = new();

        GrabNextByExpecting(TokenType.LeftParen);
        while (!_tokens.NextIsOfType(TokenType.RightParen))
        {
            string type = GrabTypeName();
            string name = GrabNextByExpecting(TokenType.Identifier);

            Token? peek = _tokens.PeekToken();

            if (peek == null)
            {
                throw new CancelParsingException("Premature EOF while parsing argument list");
            }
            
            if (peek.Type == TokenType.Comma)
            {
                _tokens.Skip(); // ,
                continue;
            }
            else if (peek.Type != TokenType.RightParen)
            {
                throw new ParseException($"Expected comma or closing paren, got {peek}", peek);
            }
        }
        GrabNextByExpecting(TokenType.RightParen);

        return vars.ToArray();
    }
    
    /// <summary>
    /// Utility method for parsing using directives.
    /// </summary>
    private string[] GrabUsingDirectives()
    {
        List<string> dependencies = new();
        
        while (_tokens.NextIsOfType(TokenType.Using))
        {
            _tokens.Skip(); // using

            try
            {
                dependencies.Add(GrabModuleName());
            }
            catch (ParseException ex)
            {
                LogErrorAndRecover(ex, false);
            }
            
            RequireSemicolon();
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
                RequireSemicolon();
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
                $"Expected module name statement, got {next?.Type.ToString() ?? "EOF"}",
                next?.Location ?? new FileLocation(_path)
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
            MessageCollection.Error("Expected module name", fail?.Location ?? new FileLocation(_path));
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
                    throw new ParseException("Expected identifier in module name", offending);
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
    private string GrabTypeName()
    {
        // In all cases we need an identifier
        // If the token after it is a double colon, we get module name first

        StringBuilder builder = new();
        
        // hack to peek with 2 tokens instead of one
        // we don't get Invalid type tokens in the lexer
        // gg ez
        if (_tokens.PeekToken(2)!.Type == TokenType.DoubleColon
            || _tokens.PeekToken(2)!.Type == TokenType.Dot)
        {
            builder.Append(GrabModuleName());

            if (_tokens.NextIsOfType(TokenType.Dot))
            {
                _tokens.Skip(); // .
            }
            else
            {
                throw new ParseException(
                    $"Expected type name, got module name {builder}",
                    _tokens.CurrentToken
                );
            }
        }

        string name = builder.Append(GrabNextByExpecting(TokenType.Identifier)).ToString();
        return name switch
        {
            "int" => "std.Integer",
            "double" => "std.Double",
            "bool" => "std.Boolean",
            _ => name
        };
    }

    /// <summary>
    /// Understands what the visibility of a symbols is by looking at modifiers.
    /// </summary>
    private GetAccessibility VisibilityFromModifiers(string[] modifiers)
    {
        if (modifiers.Contains("public"))
        {
            return GetAccessibility.Public;
        }
        else if (modifiers.Contains("protected"))
        {
            return GetAccessibility.Protected;
        }
        else if (modifiers.Contains("private"))
        {
            return GetAccessibility.Private;
        }
        else
        {
            if (!modifiers.Contains("internal"))
            {
                MessageCollection.Warn(
                    "Always specify visibility. Using internal.",
                    _tokens.CurrentToken.Location
                );
            }

            return GetAccessibility.Internal;
        }
    }
    
    /// <summary>
    /// Utility method for requiring a semicolon.
    /// </summary>
    private void RequireSemicolon()
    {
        if (_tokens.TryExpect(TokenType.Semicolon, out Token? fail))
        {
            _tokens.Skip();
        }
        else
        {
            MessageCollection.Error("Expected semicolon", fail?.Location ?? new FileLocation(_path));
        }
    }

    /// <summary>
    /// Adds an error for every modifier that's not in the allowed modifiers.
    /// </summary>
    /// <param name="modifiers">Modifiers to check.</param>
    /// <param name="errorToken">The token to use for error reporting.</param>
    /// <param name="allowed">Allowed modifiers.</param>
    private void ApplyModifierFilters(string[] modifiers, Token errorToken, params string[] allowed)
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
    private FileParser CreateSubParser()
    {
        return new FileParser(new Tokens(_tokens), _path);
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
        while (!_tokens.NextIsOfType(TokenType.RightBrace) && !_tokens.TryExpect(TokenType.Semicolon, out Token tok))
        {
            if (tok == null) return;

            _tokens.Skip();
        }

        if (recoverConsumes)
        {
            _tokens.Skip(); // we don't actually skip the semicolon/brace itself in the loop
        }
    }
}