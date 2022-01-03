using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class StringNode : AstNode
{
    public string Value { get; }

    public StringNode(ParserRuleContext context, string value) : base(context)
    {
        Value = value;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitStringNode(this);
    }
}