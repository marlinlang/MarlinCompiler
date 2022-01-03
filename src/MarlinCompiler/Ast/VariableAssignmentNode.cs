using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class VariableAssignmentNode : AstNode
{
    public MemberAccessNode Member { get; }
    public AstNode Value { get; }

    public VariableAssignmentNode(ParserRuleContext context, MemberAccessNode member, AstNode value)
        : base(context)
    {
        Member = member;
        Value = value;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitVariableAssignmentNode(this);
    }
}