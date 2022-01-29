using static MarlinCompiler.Frontend.Lexer;

namespace MarlinCompiler.Frontend;

public class Tokens
{
    /// <summary>
    /// Is there a next token, or have we reached EOF?
    /// </summary>
    public bool HasNext => PeekToken() != null;

    /// <summary>
    /// The last token. Useful for premature-EOF errors.
    /// </summary>
    public Token LastToken => _tokens.Last();

    /// <summary>
    /// The current token;
    /// </summary>
    public Token CurrentToken => _tokens[Math.Clamp(_position, 0, _tokens.Length-1)];

    private readonly Token[] _tokens;
    private int _position;

    public Tokens(Token[] tokens)
    {
        _tokens = tokens;
        _position = -1;
    }

    /// <summary>
    /// Returns the next token and advances the position. Null for EOF
    /// </summary>
    public Token GrabToken() =>
        ++_position >= _tokens.Length
            ? null
            : _tokens[_position];

    /// <summary>
    /// Returns the next token without advancing the position. Null for EOF
    /// </summary>
    public Token PeekToken() =>
        _position + 1 >= _tokens.Length
            ? null
            : _tokens[_position + 1];

    /// <summary>
    /// Skips a token.
    /// </summary>
    public void Skip() => _position++;

    /// <summary>
    /// Tries to peek and returns the next token if it matches the type of the expected.
    /// </summary>
    /// <param name="expected">Expected type</param>
    /// <param name="tok">Variable to which the token will be assigned.</param>
    /// <returns>Whether or not the next token is of the given type. False for no next token.</returns>
    public bool TryExpect(TokenType expected, out Token? tok)
    {
        Token peek = PeekToken();

        if (peek == null)
        {
            tok = null;
            return false;
        }
        else
        {
            tok = peek;
            return peek.Type == expected;
        }
    }

    /// <summary>
    /// Peeks the next token type. Skips the specified tokens.
    /// Useful for e.g. getting the token after a set of modifiers.
    /// <remarks>Does NOT advance the token position.</remarks>
    /// </summary>
    /// <param name="toSkip">The token type to skip.</param>
    /// <param name="skipAfter">After the skipped by toSkip tokens,
    /// how many additional tokens to skip</param>
    /// <returns>The next non-skipped token or null for EOF</returns>
    public Token? PeekNextTokenBySkipping(TokenType toSkip, int skipAfter)
    {
        int startPos = _position + 1;
        for (int i = _position + 1; i < _tokens.Length; i++)
        {
            if (_tokens[i].Type != toSkip)
            {
                startPos = i;
                break;
            }
        }

        if (_tokens.Length > startPos + skipAfter)
        {
            return _tokens[startPos + skipAfter];
        }

        return null;
    }

    /// <summary>
    /// Returns whether or not the next token is of the expected type.
    /// Does not advance position.
    /// </summary>
    public bool NextIsOfType(TokenType type) => PeekToken()?.Type == type;
}