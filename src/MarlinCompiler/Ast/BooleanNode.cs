using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class BooleanNode : AstNode
{
    public bool Value { get; }

    public BooleanNode(ParserRuleContext context, bool value) : base(context)
    {
        Value = value;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitBooleanNode(this);
    }
}