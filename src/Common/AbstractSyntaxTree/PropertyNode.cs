using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A class property.
/// </summary>
public class PropertyNode : VariableNode
{
    public PropertyNode(
        TypeReferenceNode type,
        string name,
        bool isStatic,
        ExpressionNode? value,
        GetAccessibility get,
        SetAccessibility set) : base(type, name, value)
    {
        IsStatic         = isStatic;
        GetAccessibility = get;
        SetAccessibility = set;
    }

    public bool             IsStatic         { get; }
    public GetAccessibility GetAccessibility { get; }
    public SetAccessibility SetAccessibility { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.Property(this);
    }
}