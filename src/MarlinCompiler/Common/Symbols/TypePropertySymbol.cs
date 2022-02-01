namespace MarlinCompiler.Common.Symbols;

public class TypePropertySymbol : VariableSymbol
{
    /// <summary>
    /// The visibility of this property.
    /// </summary>
    public GetAccessibility GetAccessibility { get; }
    
    /// <summary>
    /// The modify accessibility of this property.
    /// </summary>
    public SetAccessibility SetAccessibility { get; }

    public TypePropertySymbol(string name, TypeSymbol? type, GetAccessibility get, SetAccessibility set)
        : base(name, type)
    {
        GetAccessibility = get;
        SetAccessibility = set;
    }
}