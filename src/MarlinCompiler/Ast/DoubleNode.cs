using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class DoubleNode : AstNode
{
    public double Value { get; }

    public DoubleNode(ParserRuleContext context, double value) : base(context)
    {
        Value = value;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitDoubleNode(this);
    }
}