namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a symbol that has a name.
/// </summary>
public class NamedSymbol : ISymbol
{
    public NamedSymbol(string name)
    {
        Name = name;
    }
    
    public virtual string Name { get; }
}