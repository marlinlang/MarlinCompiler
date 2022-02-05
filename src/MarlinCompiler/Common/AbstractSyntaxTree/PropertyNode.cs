using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A class property.
/// </summary>
public class PropertyNode : VariableNode
{
    public bool IsStatic { get; }
    public bool IsNative { get; }
    public GetAccessibility GetAccessibility { get; }
    public SetAccessibility SetAccessibility { get; }

    public PropertyNode(TypeReferenceNode type, string name, bool isStatic, bool isNative,
        ExpressionNode? value, GetAccessibility get, SetAccessibility set) : base(type, name, value)
    {
        IsStatic = isStatic;
        IsNative = isNative;
        GetAccessibility = get;
        SetAccessibility = set;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.Property(this);
    }
}