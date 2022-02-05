namespace MarlinCompiler.Common.Semantics.Symbols;

public class VariableSymbol : Symbol
{
    public TypeReferenceSymbol? Type { get; }
    public string Name { get; }
    
    public VariableSymbol(TypeReferenceSymbol? type, string name)
    {
        Type = type;
        Name = name;
    }
}