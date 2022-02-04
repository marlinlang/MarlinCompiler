using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public sealed class ArrayInitializerNode : InitializerNode
{
    public ExpressionNode Length { get; }
    
    public ArrayInitializerNode(TypeReferenceNode type, ExpressionNode length) : base(type)
    {
        Length = length;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.CreateArray(this);
    }
}