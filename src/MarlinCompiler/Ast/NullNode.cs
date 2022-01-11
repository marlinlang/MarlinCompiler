using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class NullNode : AstNode
{
    public NullNode(ParserRuleContext? context) : base(context)
    {
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitNullNode(this);
    }
}