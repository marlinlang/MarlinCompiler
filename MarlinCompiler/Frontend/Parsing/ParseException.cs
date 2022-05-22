using MarlinCompiler.Frontend.Lexing;

namespace MarlinCompiler.Frontend.Parsing;

/// <summary>
/// An exception for parse errors.
/// </summary>
public class ParseException : Exception
{
    /// <summary>
    /// The token that caused the error.
    /// </summary>
    public Lexer.Token OffendingToken { get; }

    public ParseException(string errorMessage, Lexer.Token offendingToken) : base(errorMessage)
    {
        OffendingToken = offendingToken;
    }
}