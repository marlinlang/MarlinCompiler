namespace MarlinCompiler.Symbols;

public sealed class MethodSymbol : Symbol
{
    public string Name { get; }
    public bool IsStatic { get; }
    public MemberVisibility Visibility { get; }
    public List<Symbol> Args { get; }

    public MethodSymbol(string name, bool isStatic, MemberVisibility visibility, List<Symbol> args) : base(name)
    {
        Name = name;
        IsStatic = isStatic;
        Visibility = visibility;
        Args = args;
    }
}