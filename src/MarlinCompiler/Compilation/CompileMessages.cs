using System.Collections;
using Antlr4.Runtime;

namespace MarlinCompiler.Compilation;

public class CompileMessages : IEnumerable<CompileMessage>
{
    public List<CompileMessage> Contents { get; } = new();

    public int Count => Contents.Count;
    
    public bool HasErrors => Contents.Any(msg => msg.Level == CompileMessageLevel.Error);

    public void Info(string msg, FileLocation? cause = null)
        => Contents.Add(new CompileMessage(CompileMessageLevel.Message, msg, cause));

    public void Warning(string msg, FileLocation? cause = null)
        => Contents.Add(new CompileMessage(CompileMessageLevel.Warning, msg, cause));

    public void Error(string msg, FileLocation? cause = null)
        => Contents.Add(new CompileMessage(CompileMessageLevel.Error, msg, cause));

    public IEnumerator<CompileMessage> GetEnumerator() => Contents.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Contents.GetEnumerator();

    public void LoadMessages(CompileMessages messages)
    {
        Contents.AddRange(messages.Contents);
    }
}