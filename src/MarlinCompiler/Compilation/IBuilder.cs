namespace MarlinCompiler.Compilation;

public interface IBuilder
{
    string CurrentFile { get; }
    CompileMessages Messages { get; }
    bool Build(string path);
}