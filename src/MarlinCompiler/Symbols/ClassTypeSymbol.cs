namespace MarlinCompiler.Symbols;

public sealed class ClassTypeSymbol : TypeSymbol
{
    public bool IsStatic { get; }
    public bool IsSealed { get; }
    public string[] BaseClasses { get; }

    public ClassTypeSymbol(string name, MemberVisibility visibility, bool isStatic, bool isSealed,
        string[] baseClasses) : base(name, visibility)
    {
        IsStatic = isStatic;
        IsSealed = isSealed;
        BaseClasses = baseClasses;
    }
}