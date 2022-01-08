using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class MethodCallNode : AstNode
{
    public MemberAccessNode Member { get; }
    public List<AstNode> Args { get; }
    public bool IsNative { get; }

    public MethodCallNode(ParserRuleContext context, MemberAccessNode member, List<AstNode> args)
        : base(context)
    {
        Member = member;
        Args = args;
        IsNative = false;
    }

    /// <summary>
    /// To be used only with native calls
    /// </summary>
    public MethodCallNode(ParserRuleContext context, List<AstNode> args) : base(context)
    {
        Member = null;
        Args = args;
        IsNative = true;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitMethodCallNode(this);
    }
}