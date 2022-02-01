namespace MarlinCompiler.Common.Symbols;

public class TypePropertySymbol : VariableSymbol
{
    /// <summary>
    /// Is the property static?
    /// </summary>
    public bool IsStatic { get; }
    
    /// <summary>
    /// Is the property's value native?
    /// </summary>
    public bool IsNative { get; }

    /// <summary>
    /// The visibility of this property.
    /// </summary>
    public GetAccessibility GetAccessibility { get; }
    
    /// <summary>
    /// The modify accessibility of this property.
    /// </summary>
    public SetAccessibility SetAccessibility { get; }

    public TypePropertySymbol(string name, TypeSymbol? type, bool isStatic, bool isNative,
        GetAccessibility get, SetAccessibility set) : base(name, type)
    {
        IsStatic = isStatic;
        IsNative = isNative;
        GetAccessibility = get;
        SetAccessibility = set;
    }
}