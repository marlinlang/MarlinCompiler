using Antlr4.Runtime;

namespace MarlinCompiler.Compilation;

public class CompileMessage
{
    public CompileMessageLevel Level { get; }
    public string Message { get; }
    public FileLocation? Location { get; }

    public CompileMessage(CompileMessageLevel level, string message, FileLocation? location = null)
    {
        Level = level;
        Message = message;
        Location = location;
    }

    public override string ToString()
    {
        if (Location != null)
        {
            return $"{Location.ToString()}: {Message}";
        }
        else
        {
            return Message;
        }
    }
}

public class FileLocation
{
    public string Path { get; }
    public int Line { get; }
    public int Column { get; }

    public FileLocation(string path, int line, int column)
    {
        Path = path;
        Line = line;
        Column = column;
    }

    public FileLocation(IBuilder builder, IToken token)
    {
        Path = builder.CurrentFile;
        Line = token.Line;
        Column = token.Column + 1;
    }
    
    public override string ToString() => $"{Path}:{Line}:{Column}";
}

public enum CompileMessageLevel
{
    Message,
    Warning,
    Error
}