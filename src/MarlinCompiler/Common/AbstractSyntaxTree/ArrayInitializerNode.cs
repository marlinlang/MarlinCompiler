using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public sealed class ArrayInitializerNode : InitializerNode
{
    public ArrayInitializerNode(TypeReferenceNode type, ExpressionNode length) : base(type)
    {
        Length = length;
    }
    
    public ExpressionNode Length { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.CreateArray(this);
    }
}