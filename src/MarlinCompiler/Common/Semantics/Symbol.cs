namespace MarlinCompiler.Common.Semantics;

/// <summary>
/// Represents a symbol and its metadata,
/// </summary>
public class Symbol
{
    #region Main data
    
    /// <summary>
    /// The name of the symbol.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The (Marlin) type of the symbol, e.g. std::Integer
    /// </summary>
    public string Type { get; }
    
    /// <summary>
    /// The kind of the symbol.
    /// </summary>
    public SymbolKind Kind { get; }
    
    #endregion
    
    #region Metadata

    public Symbol? TypeSymbol { get; set; } = null;

    /// <summary>
    /// Used for symbols for types, etc.
    /// </summary>
    public Scope? AttachedScope { get; set; } = null;
    
    public GetAccessibility GetAccess { get; set; } = GetAccessibility.Internal;
    public SetAccessibility SetAccess { get; set; } = SetAccessibility.NoModify;
    public bool IsStatic { get; set; } = false;
    public string[] MethodSignature { get; set; } = Array.Empty<string>();
    
    #endregion

    public Symbol(string name, string type, SymbolKind kind)
    {
        Name = name;
        Type = type;
        Kind = kind;
    }

    public override string ToString()
    {
        return $"<{Name}, {Type}, {Kind}>";
    }
}