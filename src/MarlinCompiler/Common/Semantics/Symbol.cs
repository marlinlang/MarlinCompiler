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

    /// <summary>
    /// For properties and methods, this is the symbol of the instance variable.
    /// </summary>
    public Symbol? AccessInstance { get; set; } = null;
    
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

    public static bool operator ==(Symbol? a, Symbol? b)
    {
        return a?.Equals(b) ?? (a is null && b is null); // ?? is necessary because of a
    }

    public static bool operator !=(Symbol? a, Symbol? b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        return obj != null && obj.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode() => $"{ToString()};{GetAccess};{SetAccess};{IsStatic}".GetHashCode();

    public override string ToString()
    {
        return $"<{Name}, {Type}, {Kind}>";
    }
}