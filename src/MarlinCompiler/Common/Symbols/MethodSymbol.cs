namespace MarlinCompiler.Common.Symbols;

public sealed class MethodSymbol : Symbol
{
    /// <summary>
    /// The return type of this method.
    /// </summary>
    public TypeSymbol? Type { get; }
    
    /// <summary>
    /// The expected arguments for this method.
    /// </summary>
    public List<VariableSymbol> Args { get; }

    public MethodSymbol(string name, TypeSymbol? type, List<VariableSymbol> args) : base(name)
    {
        Type = type;
        Args = args;
    }
}