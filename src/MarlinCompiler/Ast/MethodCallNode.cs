using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class MethodCallNode : AstNode
{
    public MemberAccessNode Member { get; }
    public List<AstNode> Args { get; }

    public MethodCallNode(ParserRuleContext context, MemberAccessNode member, List<AstNode> args)
        : base(context)
    {
        Member = member;
        Args = args;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitMethodCallNode(this);
    }
}