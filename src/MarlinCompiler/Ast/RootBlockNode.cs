using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public sealed class RootBlockNode : BlockNode
{
    public RootBlockNode() : base(null)
    {
    }
}