namespace MarlinCompiler.Symbols;

public class TypeSymbol : Symbol
{
    public MemberVisibility Visibility { get; }

    public override string UserType => "type";

    protected TypeSymbol(string name, MemberVisibility visibility) : base(name)
    {
        Visibility = visibility;
    }
}