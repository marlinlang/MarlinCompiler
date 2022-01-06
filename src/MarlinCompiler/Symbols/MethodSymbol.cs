namespace MarlinCompiler.Symbols;

public sealed class MethodSymbol : Symbol
{
    public string Name { get; }
    public TypeSymbol Type { get; set; }
    public bool IsStatic { get; }
    public MemberVisibility Visibility { get; }
    public List<Symbol> Args { get; }

    public override string UserType => "method";

    public MethodSymbol(string name, bool isStatic, MemberVisibility visibility, List<Symbol> args) : base(name)
    {
        Name = name;
        IsStatic = isStatic;
        Visibility = visibility;
        Args = args;
    }
}