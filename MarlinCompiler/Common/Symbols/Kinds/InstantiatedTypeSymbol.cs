namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents an usage of a type.
/// </summary>
public class TypeUsageSymbol : ISymbol
{
    public TypeUsageSymbol(TypeSymbol? type)
    {
        Type        = type;
        GenericArgs = Array.Empty<TypeSymbol>();
    }
    
    /// <summary>
    /// The type that was instantiated.
    /// </summary>
    public TypeSymbol? Type { get; set; }
    
    /// <summary>
    /// The generic argument which was passed.
    /// </summary>
    public TypeSymbol[] GenericArgs { get; set; }
}