namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents an usage of a type.
/// </summary>
public class TypeUsageSymbol : ISymbol
{
    public TypeUsageSymbol(TypeSymbol type, bool typeReferencedStatically = false)
    {
        Type                = type;
        TypeReferencedStatically = typeReferencedStatically;
        GenericArgs         = Array.Empty<TypeSymbol>();
    }
    
    /// <summary>
    /// The type that was instantiated.
    /// </summary>
    public TypeSymbol Type { get; set; }

    /// <summary>
    /// Whether the type was used statically or not.
    /// </summary>
    public bool TypeReferencedStatically { get; set; }

    /// <summary>
    /// The generic argument which was passed.
    /// </summary>
    public TypeSymbol[] GenericArgs { get; set; }
}