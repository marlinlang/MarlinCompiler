using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class MemberAccessNode : AstNode
{
    public AstNode? ArrayIndex { get; set; }
    public AstNode? Parent { get; set; }
    public AstNode Member { get; }
    
    public MemberAccessNode(ParserRuleContext context, AstNode? parent, AstNode member, AstNode? arrayIndex)
        : base(context)
    {
        Parent = parent;
        Member = member;
        ArrayIndex = arrayIndex;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitMemberAccessNode(this);
    }
}