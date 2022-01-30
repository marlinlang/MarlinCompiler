using System.Text;
using System.Text.RegularExpressions;
using MarlinCompiler.Common;

namespace MarlinCompiler.Frontend;

/// <summary>
/// Basic lexer implementation.
/// </summary>
public sealed class Lexer
{
    /// <summary>
    /// A representation of a token.
    /// </summary>
    public record Token(TokenType Type, string Value, FileLocation Location)
    {
        /// <summary>
        /// The operator precedence of this token
        /// </summary>
        public int Precedence => Type switch
        {
            TokenType.Dot => 10,
            TokenType.Plus => 10,
            
            _ => 0
        };

        /// <summary>
        /// Is this a right-associative operator?
        /// </summary>
        public bool IsRightAssocBinOp => Type switch
        {
            TokenType.Power => true,
            
            _ => false
        };
    }
    
    /// <summary>
    /// A definition to use to match a token.
    /// </summary>
    public record TokenDefinition(TokenType TokenType, Regex Regex);
    
    /// <summary>
    /// The tokens in the language.
    /// </summary>
    private static readonly TokenDefinition[] TokenDefinitions = new[]
    {
        new TokenDefinition(TokenType.Skip,   new Regex("^\\/\\/.*", RegexOptions.Compiled)),
        
        new TokenDefinition(TokenType.Module,       new Regex("^module\\b", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Class,        new Regex("^class\\b", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Struct,       new Regex("^struct\\b", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Using,        new Regex("^using\\b", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.New,          new Regex("^new\\b", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Get,          new Regex("^get\\b", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Set,          new Regex("^set\\b", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Operator,     new Regex("^operator\\b", RegexOptions.Compiled)),
        
        new TokenDefinition(TokenType.Modifier,     new Regex(
            "^(public|private|protected|internal|readonly|static)\\b", RegexOptions.Compiled)
        ),
        
        new TokenDefinition(TokenType.String,       new Regex("^\".*\"", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Character,    new Regex("^'\\\\?.'", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Boolean,      new Regex("^(true|false)\\b", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Decimal,      new Regex("^[-+]?[0-9]*\\.[0-9]+", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Integer,      new Regex("^[-+]?(0x)?[0-9]+", RegexOptions.Compiled)),
        
        new TokenDefinition(TokenType.Arrow,        new Regex("^->", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.And,          new Regex("^&&", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Or,           new Regex("^\\|\\|", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.DoubleColon,  new Regex("^::", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Assign,       new Regex("^=")),
        new TokenDefinition(TokenType.Equal,        new Regex("^==")),
        new TokenDefinition(TokenType.NotEqual,     new Regex("^!=")),
        new TokenDefinition(TokenType.Plus,         new Regex("^\\+", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Minus,        new Regex("^-", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Asterisk,     new Regex("^\\*", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Slash,        new Regex("^\\\\", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Colon,        new Regex("^:", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Question,     new Regex("^\\?", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Ampersand,    new Regex("^&", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Comma,        new Regex("^,", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Dot,          new Regex("^\\.", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Semicolon,    new Regex("^;", RegexOptions.Compiled)),
        
        new TokenDefinition(TokenType.LeftParen,    new Regex("^\\(", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.RightParen,   new Regex("^\\)", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.LeftBrace,    new Regex("^\\{", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.RightBrace,   new Regex("^\\}", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.LeftBracket,  new Regex("^\\[", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.RightBracket, new Regex("^\\]", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.LeftAngle,    new Regex("^<", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.RightAngle,   new Regex("^>", RegexOptions.Compiled)),
        new TokenDefinition(TokenType.Identifier,   new Regex("^((\\p{L}|_)(\\p{L}|[0-9_])*)", RegexOptions.Compiled)),
    };

    private readonly StringBuilder _parseContent;
    private readonly string _filePath;
    private readonly string _startingContent;
    private readonly int _startingLength;

    private FileLocation CurrentLocation
    {
        get
        {
            int currentPos = _startingLength - _parseContent.Length;
            int line = 1;
            int col = 0;
            for (int i = 0; i < currentPos; i++)
            {
                if (_startingContent[i] == '\r') continue;
                
                if (_startingContent[i] == '\n')
                {
                    line++;
                    col = 0;
                }
                else
                {
                    col++;
                }
            }

            return new FileLocation(_filePath, line, col);
        }
    }
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="source">The text to parse.</param>
    /// <param name="filePath">The file path of the text. Only used for error reporting.</param>
    public Lexer(string source, string filePath)
    {
        _parseContent = new StringBuilder(source.Trim());
        _filePath = filePath;
        _startingContent = source;
        _startingLength = source.Length;
    }

    public Token[] Lex()
    {
        List<Token> tokens = new();

        for (Token? current = null; (current = Match()) != null; )
        {
            if (current.Type != TokenType.Skip)
            {
                tokens.Add(current);
            }
        }

        return tokens.ToArray();
    }

    private Token? Match()
    {
        string useString = _parseContent.ToString();
        
        string trimmed = useString.TrimStart();
        int leadingSpaces = useString.Length - trimmed.Length;
        useString = trimmed;

        if (useString.Length == 0)
        {
            return null;
        }
        
        foreach (TokenDefinition definition in TokenDefinitions)
        {
            Match match = definition.Regex.Match(useString);
            if (match.Success)
            {
                // Don't match the same token again
                _parseContent.Remove(0, leadingSpaces + match.Length);

                return new Token(definition.TokenType, match.Value, CurrentLocation);
            }
        }

        _parseContent.Remove(0, leadingSpaces + 1);
        return new Token(TokenType.Invalid, useString[0].ToString(), CurrentLocation);
    }
}