namespace MarlinCompiler.Common.Symbols;

/// <summary>
/// Representation of a type in the symbol table.
/// Meant to be inherited from.
/// </summary>
public class TypeSymbol : Symbol
{
    /// <summary>
    /// The module that this type is contained in.
    /// </summary>
    public string Module { get; }

    /// <summary>
    /// To what extent is this type visible?
    /// </summary>
    public GetAccessibility Accessibility { get; }

    protected TypeSymbol(string name, string module, GetAccessibility accessibility) : base(name)
    {
        Module = module;
        Accessibility = accessibility;
    }
}