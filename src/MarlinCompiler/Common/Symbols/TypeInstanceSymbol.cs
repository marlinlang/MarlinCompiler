namespace MarlinCompiler.Common.Symbols;

/// <summary>
/// Represents an instance of a type.
/// </summary>
public sealed class TypeInstanceSymbol : Symbol
{
    public TypeSymbol? TypeOf { get; }

    public TypeInstanceSymbol(TypeSymbol? typeOf) : base($"instance.{typeOf?.Name ?? "<???>"}")
    {
        TypeOf = typeOf;
    }
}