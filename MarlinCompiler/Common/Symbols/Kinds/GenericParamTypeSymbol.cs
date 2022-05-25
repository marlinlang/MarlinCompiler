namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a generic param type.
/// </summary>
public sealed class GenericParamTypeSymbol : TypeSymbol
{
    public GenericParamTypeSymbol(string name, ClassTypeSymbol owner)
        : base(String.Empty, name, GetAccessibility.Protected)
    {
        Owner = owner;
    }

    public override string Name => TypeName;

    /// <summary>
    /// The class that has this generic param.
    /// </summary>
    public ClassTypeSymbol Owner { get; set; }
}