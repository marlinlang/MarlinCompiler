namespace MarlinCompiler.Common.Semantics.Symbols;

public class PropertyVariableSymbol : VariableSymbol
{
    public bool IsStatic { get; }
    public bool IsNative { get; }
    public GetAccessibility GetAccessibility { get; }
    public SetAccessibility SetAccessibility { get; }

    public PropertyVariableSymbol(TypeReferenceSymbol? type, string name, bool isStatic, bool isNative,
        GetAccessibility getAccessibility, SetAccessibility setAccessibility) : base(type, name)
    {
        IsStatic = isStatic;
        IsNative = isNative;
        GetAccessibility = getAccessibility;
        SetAccessibility = setAccessibility;
    }
}