using System.Text;
using System.Text.RegularExpressions;
using MarlinCompiler.Common;
using MarlinCompiler.Common.Messages;

namespace MarlinCompiler.Frontend.Lexing;

/// <summary>
/// Basic lexer implementation.
/// </summary>
public sealed class Lexer
{
    public Lexer(string sourceText, string filePath)
    {
        MessageCollection = new MessageCollection();

        _parseContent    = new StringBuilder(sourceText.Trim());
        _filePath        = filePath;
        _startingContent = sourceText;
        _startingLength  = sourceText.Length;
    }

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
            TokenType.Dot  => 10,
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

        public override int GetHashCode()
        {
            return $"TOK<{Type}>({Value};{Value.GetHashCode()})".GetHashCode();
        }

        public virtual bool Equals(Token? other) => other is not null && other.Type == Type && other.Value == Value;
    }

    /// <summary>
    /// A definition to use to match a token.
    /// </summary>
    public record TokenDefinition(TokenType TokenType, Regex Regex);

    /// <summary>
    /// The tokens in the language.
    /// </summary>
    private static readonly TokenDefinition[] TokenDefinitions =
    {
        new(TokenType.Skip, new Regex("^\\/\\/.*", RegexOptions.Compiled)),
        new(TokenType.Skip, new Regex("^\\/\\*(.|\n)*\\*\\/", RegexOptions.Multiline)),

        new(TokenType.Module, new Regex("^module\\b", RegexOptions.Compiled)),
        new(TokenType.Class, new Regex("^class\\b", RegexOptions.Compiled)),
        new(TokenType.Struct, new Regex("^struct\\b", RegexOptions.Compiled)),
        new(TokenType.Constructor, new Regex("^constructor\\b", RegexOptions.Compiled)),
        new(TokenType.Using, new Regex("^using\\b", RegexOptions.Compiled)),
        new(TokenType.New, new Regex("^new\\b", RegexOptions.Compiled)),
        new(TokenType.Get, new Regex("^get\\b", RegexOptions.Compiled)),
        new(TokenType.Set, new Regex("^set\\b", RegexOptions.Compiled)),
        new(TokenType.Mutable, new Regex("^mut\\b", RegexOptions.Compiled)),
        new(TokenType.Void, new Regex("^void\\b", RegexOptions.Compiled)),
        new(TokenType.Null, new Regex("^null\\b", RegexOptions.Compiled)),
        new(TokenType.Extern, new Regex("^extern\\b", RegexOptions.Compiled)),
        new(TokenType.Return, new Regex("^return\\b", RegexOptions.Compiled)),
        new(TokenType.Operator, new Regex("^operator\\b", RegexOptions.Compiled)),

        new(TokenType.Modifier, new Regex("^(public|private|protected|internal|static)\\b", RegexOptions.Compiled)),

        new(TokenType.String, new Regex("^\".*\"", RegexOptions.Compiled)),
        new(TokenType.Character, new Regex("^'\\\\?.'", RegexOptions.Compiled)),
        new(TokenType.Boolean, new Regex("^(true|false)\\b", RegexOptions.Compiled)),
        new(TokenType.Decimal, new Regex("^[-+]?[0-9]*\\.[0-9]+", RegexOptions.Compiled)),
        new(TokenType.Integer, new Regex("^[-+]?(0x)?[0-9]+", RegexOptions.Compiled)),

        new(TokenType.Arrow, new Regex("^->", RegexOptions.Compiled)),
        new(TokenType.And, new Regex("^&&", RegexOptions.Compiled)),
        new(TokenType.Or, new Regex("^\\|\\|", RegexOptions.Compiled)),
        new(TokenType.DoubleColon, new Regex("^::", RegexOptions.Compiled)),
        new(TokenType.Assign, new Regex("^=")),
        new(TokenType.Equal, new Regex("^==")),
        new(TokenType.NotEqual, new Regex("^!=")),
        new(TokenType.At, new Regex("^@")),
        new(TokenType.Plus, new Regex("^\\+", RegexOptions.Compiled)),
        new(TokenType.Minus, new Regex("^-", RegexOptions.Compiled)),
        new(TokenType.Asterisk, new Regex("^\\*", RegexOptions.Compiled)),
        new(TokenType.Slash, new Regex("^\\/", RegexOptions.Compiled)),
        new(TokenType.Colon, new Regex("^:", RegexOptions.Compiled)),
        new(TokenType.Question, new Regex("^\\?", RegexOptions.Compiled)),
        new(TokenType.Ampersand, new Regex("^&", RegexOptions.Compiled)),
        new(TokenType.Comma, new Regex("^,", RegexOptions.Compiled)),
        new(TokenType.Dot, new Regex("^\\.", RegexOptions.Compiled)),
        new(TokenType.Semicolon, new Regex("^;", RegexOptions.Compiled)),

        new(TokenType.LeftParen, new Regex("^\\(", RegexOptions.Compiled)),
        new(TokenType.RightParen, new Regex("^\\)", RegexOptions.Compiled)),
        new(TokenType.LeftBrace, new Regex("^\\{", RegexOptions.Compiled)),
        new(TokenType.RightBrace, new Regex("^\\}", RegexOptions.Compiled)),
        new(TokenType.LeftAngle, new Regex("^<", RegexOptions.Compiled)),
        new(TokenType.RightAngle, new Regex("^>", RegexOptions.Compiled)),
        new(TokenType.Identifier, new Regex("^((\\p{L}|_)(\\p{L}|[0-9_])*)", RegexOptions.Compiled))
    };

    public MessageCollection MessageCollection { get; }

    private readonly StringBuilder _parseContent;
    private readonly string        _filePath;
    private readonly string        _startingContent;
    private readonly int           _startingLength;

    private FileLocation CurrentLocation
    {
        get
        {
            int currentPos = _startingLength - _parseContent.Length;
            int line = 1;
            int col = 0;
            for (int i = 0; i < currentPos; ++i)
            {
                switch (_startingContent[i])
                {
                    case '\r':
                        continue;
                    case '\n':
                        line++;
                        col = 0;
                        break;
                    default:
                        col++;
                        break;
                }
            }

            return new FileLocation(_filePath, line, col);
        }
    }

    public Token[] Lex()
    {
        List<Token> tokens = new();

        for (Token? current; (current = Match()) != null;)
        {
            switch (current.Type)
            {
                case TokenType.Skip:
                    continue;
                case TokenType.Invalid:
                    MessageCollection.Error(MessageId.InvalidCharacter, $"Invalid character: {current.Value}", current.Location);
                    continue;
                default:
                    tokens.Add(current);
                    break;
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

                // Strings contains the quotes around them in their value
                // This could be fixed by modifying the regex but this is
                // definitely more readable and (probably) faster 
                string useValue = definition.TokenType == TokenType.String
                                      ? match.Value[1..^1]
                                      : match.Value;

                return new Token(definition.TokenType, useValue, CurrentLocation);
            }
        }

        _parseContent.Remove(0, leadingSpaces + 1);
        return new Token(TokenType.Invalid, useString[0].ToString(), CurrentLocation);
    }
}