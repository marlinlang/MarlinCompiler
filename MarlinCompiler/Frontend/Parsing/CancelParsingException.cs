namespace MarlinCompiler.Frontend.Parsing;

/// <summary>
/// Exception used to cancel the parsing process due to too many errors.
/// </summary>
public class CancelParsingException : Exception
{
    public CancelParsingException(string reason) : base(reason)
    {
    }
}