namespace MarlinCompiler.Common.Symbols;

/// <summary>
/// Represents an instance of a type.
/// </summary>
public sealed class TypeInstanceSymbol : Symbol
{
    public TypeSymbol? TypeOf { get; }

    public TypeInstanceSymbol(TypeSymbol? typeOf) : base($"{typeOf?.Name ?? "<???>"}.inst")
    {
        TypeOf = typeOf;
    }

    public override Symbol? FindInChildren(string name)
    {
        if (TypeOf == default)
        {
            return base.FindInChildren(name);
        }

        return TypeOf.FindInChildren(name);
    }

    public override Symbol? Find(Predicate<Symbol> predicate)
    {
        if (TypeOf == default)
        {
            return base.Find(predicate);
        }

        return TypeOf.Find(predicate);
    }

    public override Symbol[] FindAll(Predicate<Symbol> predicate)
    {
        if (TypeOf == default)
        {
            return base.FindAll(predicate);
        }

        return TypeOf.FindAll(predicate);
    }
}