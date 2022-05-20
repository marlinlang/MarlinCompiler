namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents an usage of a type.
/// </summary>
public class TypeUsageSymbol : ISymbol
{
    /// <summary>
    /// The type that was instantiated.
    /// </summary>
    public TypeSymbol? Type { get; set; }
    
    /// <summary>
    /// The generic argument which was passed.
    /// </summary>
    public TypeSymbol? GenericArg { get; set; }
}