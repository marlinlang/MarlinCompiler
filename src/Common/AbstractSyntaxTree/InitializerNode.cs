using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A node making a new instance of something (i.e. the 'new' keyword).
/// </summary>
public class InitializerNode : IndexableExpressionNode
{
    protected InitializerNode(TypeReferenceNode type)
    {
        Type = type;
    }

    public TypeReferenceNode Type { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        throw new InvalidOperationException();
    }
}