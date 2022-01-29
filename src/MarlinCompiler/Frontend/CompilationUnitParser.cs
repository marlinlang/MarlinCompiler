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
public sealed class CompilationUnitParser
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
    public CompilationUnitParser(Tokens tokens, string filePath)
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
        if (!_tokens.HasNext) return null;

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
                    Node child = ExpectTypeDefinition();

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
    private TypeDefinitionNode ExpectTypeDefinition()
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
        CompilationUnitParser tempParser = new(new Tokens(_tokens), _path);
        
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
        Token accessibilityErrTok = _tokens.CurrentToken;
        
        string? baseClass = null;
        if (_tokens.NextIsOfType(TokenType.Colon))
        {
            _tokens.Skip(); // colon
            baseClass = GrabNextByExpecting(TokenType.Identifier);
        }
        
        ClassTypeDefinitionNode classNode = new ClassTypeDefinitionNode(
            name,
            MakeAccessibility(modifiers, true, accessibilityErrTok),
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
        VariableNode[] args = GrabTupleTypeDefinition();

        MethodDeclarationNode node = new MethodDeclarationNode(
            MakeAccessibility(modifiers, true, nameToken),
            new TypeReferenceNode(type),
            name,
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
        Node? value = null;

        if (_tokens.NextIsOfType(TokenType.Assign))
        {
            _tokens.Skip(); // =
            value = ExpectExpression();
        }
        
        RequireSemicolon();

        return new PropertyNode(
            new TypeReferenceNode(type),
            name,
            value,
            MakeAccessibility(modifiers, false, nameToken)
        );
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

        if (_tokens.NextIsOfType(TokenType.Identifier))
        {
            // local var decl, var assignment or method call
            Node[] accessPath = GrabAccessPath();
            
            

            throw new NotImplementedException(); // temporary for the commit
        }
        else
        {
            Token? peek = _tokens.PeekToken();
            if (peek == null)
            {
                throw new CancelParsingException("Premature EOF");
            }
            
            throw new ParseException($"Expected statement, got {peek}", peek);
        }
    }

    /// <summary>
    /// Expects any expression.
    /// </summary>
    private ExpressionNode ExpectExpression()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Utility method for expecting an argument list
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
            modifiers.Add(_tokens.GrabToken().Value);
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
        
        while (_tokens.TryExpect(TokenType.Identifier, out Token? current))
        {
            _tokens.Skip(); // current token
            name.Append(current.Value);

            if (_tokens.NextIsOfType(TokenType.DoubleColon))
            {
                name.Append(_tokens.GrabToken().Value); // add separator

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
    /// Grabs an access path to a property/method.
    /// </summary>
    private Node[] GrabAccessPath()
    {
        List<Node> accessPath = new();

        throw new NotImplementedException();

        return accessPath.ToArray();
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
        if (_tokens.PeekToken(2).Type == TokenType.DoubleColon
            || _tokens.PeekToken(2).Type == TokenType.Dot)
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

        return builder.Append(GrabNextByExpecting(TokenType.Identifier)).ToString();
    }

    /// <summary>
    /// Utility method for making an Accessibility flags instance by the given modifiers.
    /// </summary>
    /// <param name="modifiers">The modifiers to work with.</param>
    /// <param name="includeModify">If the Accessibility flags should include a modify flag.</param>
    /// <param name="errorToken">If no accessibility modifiers are present, a warning will be generated.
    /// This is the token that will be used for that error's location.</param>
    private Accessibility MakeAccessibility(string[] modifiers, bool forceNoModify, Token errorToken)
    {
        Accessibility get;
        Accessibility set;
        
        if (modifiers.Contains("public"))
        {
            get = Accessibility.PublicAccess;
            set = Accessibility.PublicModify;
        }
        else if (modifiers.Contains("private"))
        {
            get = Accessibility.PrivateAccess;
            set = Accessibility.PrivateAccess;
        }
        else
        {
            if (!modifiers.Contains("internal"))
            {
                MessageCollection.Warn(
                    "Always specify visibility. Using internal visibility.",
                    errorToken.Location
                );
            }
            
            get = Accessibility.InternalAccess;
            set = Accessibility.InternalModify;
        }

        return forceNoModify
            ? get | Accessibility.NoModify
            : get | set;
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