using MarlinCompiler.Intermediate;

namespace MarlinCompiler.Common.Symbols;

public sealed class MethodSymbol : Symbol
{
    /// <summary>
    /// Is the method static?
    /// </summary>
    public bool IsStatic { get; }

    /// <summary>
    /// The return type of this method.
    /// </summary>
    public TypeSymbol? Type { get; }
    
    /// <summary>
    /// The expected arguments for this method.
    /// </summary>
    public List<VariableSymbol> Args { get; }
    
    public string Signature { get; init; }

    public MethodSymbol(string name, bool isStatic, TypeSymbol? type, List<VariableSymbol> args) : base(name)
    {
        IsStatic = isStatic;
        Type = type;
        Args = args;
        Signature = "<not_set>";
    }
}