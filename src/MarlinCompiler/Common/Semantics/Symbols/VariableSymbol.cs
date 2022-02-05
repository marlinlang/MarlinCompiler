namespace MarlinCompiler.Common.Semantics.Symbols;

public class VariableSymbol : Symbol
{
    public TypeReferenceSymbol? Type { get; }
    public string Name { get; }
    
    public bool IsInitialized { get; set; }

    public VariableSymbol(TypeReferenceSymbol? type, string name)
    {
        Type = type;
        Name = name;
        IsInitialized = false;
    }
}