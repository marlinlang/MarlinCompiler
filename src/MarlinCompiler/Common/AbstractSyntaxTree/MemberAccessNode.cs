using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A class representing accessing a local variable or member
/// </summary>
public class MemberAccessNode : IndexableExpressionNode
{
    public MemberAccessNode(string memberName)
    {
        MemberName = memberName;
    }
    
    public string MemberName { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.MemberAccess(this);
    }
}