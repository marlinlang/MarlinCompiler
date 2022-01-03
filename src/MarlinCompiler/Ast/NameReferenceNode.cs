using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public sealed class NameReferenceNode : AstNode
{
    public string Name { get; }

    public NameReferenceNode(ParserRuleContext context, string name) : base(context)
    {
        Name = name;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitNameReferenceNode(this);
    }
}