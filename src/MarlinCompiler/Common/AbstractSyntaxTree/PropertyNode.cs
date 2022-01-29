using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class PropertyNode : VariableNode
{
    public Accessibility Accessibility { get; }

    public PropertyNode(TypeReferenceNode type, string name, Node? value, Accessibility accessibility)
        : base(type, name, value)
    {
        Accessibility = accessibility;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.Property(this);
    }
}