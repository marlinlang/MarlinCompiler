using MarlinCompiler.Compilation;

namespace MarlinCompiler.MarlinCompiler.Compilation;

public class ParseException : Exception
{
    public CompileMessage CompileMessage { get; }

    public ParseException(CompileMessage compileMessage)
        : base("There was an error parsing this document.")
    {
        CompileMessage = compileMessage;
    }
}