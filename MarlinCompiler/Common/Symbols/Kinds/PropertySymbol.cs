using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

public class PropertySymbol : VariableSymbol
{
    public PropertySymbol(PropertyNode node) : base(node)
    {
        GetAccessibility = node.GetAccessibility;
        SetAccessibility = node.SetAccessibility;
        IsStatic         = node.IsStatic;
    }

    /// <summary>
    /// The get accessor of the property.
    /// </summary>
    public GetAccessibility GetAccessibility { get; }

    /// <summary>
    /// The set accessor of the property.
    /// </summary>
    public SetAccessibility SetAccessibility { get; }

    /// <summary>
    /// Whether the property is static.
    /// </summary>
    public bool IsStatic { get; }
}