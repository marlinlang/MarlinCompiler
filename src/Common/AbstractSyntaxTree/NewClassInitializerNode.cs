using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// An expression similar to new app:SomeClass().
/// </summary>
public sealed class NewClassInitializerNode : InitializerNode
{
    public NewClassInitializerNode(TypeReferenceNode type, ExpressionNode[] ctorArgs) : base(type)
    {
        ConstructorArgs = ctorArgs;
    }

    public ExpressionNode[] ConstructorArgs { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.NewClassInstance(this);
    }
}