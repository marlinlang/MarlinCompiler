using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class BlockNode : AstNode
{
    public List<AstNode> Body { get; }

    public override IEnumerable<AstNode> Children => Body;

    public BlockNode(ParserRuleContext context) : base(context)
    {
        Body = new List<AstNode>();
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitBlockNode(this);
    }
}