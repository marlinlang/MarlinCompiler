using MarlinCompiler.Common.Messages;
using MarlinCompiler.Frontend.Lexing;

namespace MarlinCompiler.Frontend.Parsing;

/// <summary>
/// An exception for parse errors.
/// </summary>
public class ParseException : Exception
{
    public ParseException(MessageId id, string errorMessage, Token offendingToken) : base(errorMessage)
    {
        OffendingToken = offendingToken;
        MessageId      = id;
    }

    /// <summary>
    /// The token that caused the error.
    /// </summary>
    public Token OffendingToken { get; }

    /// <summary>
    /// The ID of the message.
    /// </summary>
    public MessageId MessageId { get; }
}