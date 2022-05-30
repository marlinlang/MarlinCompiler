using MarlinCompiler.Common.FileLocations;

namespace MarlinCompiler.Frontend.Lexing;

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