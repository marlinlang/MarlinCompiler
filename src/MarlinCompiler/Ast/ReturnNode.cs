using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class ReturnNode : AstNode
{
    public AstNode? Value { get; }

    public ReturnNode(ParserRuleContext context, AstNode? value) : base(context)
    {
        Value = value;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitReturnNode(this);
    }
}