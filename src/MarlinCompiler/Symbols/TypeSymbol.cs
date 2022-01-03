namespace MarlinCompiler.Symbols;

public class TypeSymbol : Symbol
{
    public MemberVisibility Visibility { get; }

    protected TypeSymbol(string name, MemberVisibility visibility) : base(name)
    {
        Visibility = visibility;
    }
}