namespace MarlinCompiler.Symbols;

public sealed class MethodSymbol : Symbol
{
    public TypeSymbol? Type { get; set; }
    public bool IsStatic { get; }
    public MemberVisibility Visibility { get; }
    public List<Symbol> Args { get; }

    public override string UserType => "method";

    public MethodSymbol(string name, bool isStatic, MemberVisibility visibility, List<Symbol> args) : base(name)
    {
        IsStatic = isStatic;
        Visibility = visibility;
        Args = args;
    }
}