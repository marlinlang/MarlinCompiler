using MarlinCompiler.Ast;
using MarlinCompiler.Compilation;

namespace MarlinCompiler.MarlinCompiler.Compilation.Targets;

public abstract class BaseCompilationTarget
{
    public CompileMessages Messages { get; }

    protected BaseCompilationTarget()
    {
        Messages = new CompileMessages();
    }

    public abstract bool InvokeTarget(AstNode root);
}