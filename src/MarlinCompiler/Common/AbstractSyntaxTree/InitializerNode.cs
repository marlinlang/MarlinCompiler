using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A node making a new instance of something (i.e. the 'new' keyword).
/// </summary>
public class InitializerNode : IndexableExpressionNode
{
    public TypeReferenceNode Type { get; }

    protected InitializerNode(TypeReferenceNode type)
    {
        Type = type;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        throw new InvalidOperationException();
    }
}