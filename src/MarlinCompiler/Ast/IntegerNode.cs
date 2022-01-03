using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class IntegerNode : AstNode
{
    public int Value { get; }

    public IntegerNode(ParserRuleContext context, int value) : base(context)
    {
        Value = value;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitIntegerNode(this);
    }
}