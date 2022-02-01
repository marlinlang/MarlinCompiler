namespace MarlinCompiler.Common.Symbols;

public class VariableSymbol : Symbol
{
    public TypeSymbol? Type { get; }
    public Symbol? Value { get; set; }
    
    public VariableSymbol(string name, TypeSymbol? type) : base(name)
    {
        Type = type;
    }
}