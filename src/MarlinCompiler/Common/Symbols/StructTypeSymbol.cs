namespace MarlinCompiler.Common.Symbols;

/// <summary>
/// Representation of a struct in the symbol table.
/// </summary>
public sealed class StructTypeSymbol : TypeSymbol
{
    public StructTypeSymbol(string name, string module, GetAccessibility accessibility)
        : base(name, module, accessibility)
    {
    }
}