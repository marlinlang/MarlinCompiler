namespace MarlinCompiler.Symbols;

public sealed class VariableSymbol : Symbol
{
    public string Type { get; }
    public MemberVisibility Visibility { get; }
    public bool IsStatic { get; }

    public override string UserType => "variable";
    
    /// <summary>
    /// Used for member & local variables.
    /// </summary>
    public VariableSymbol(string name, string type, MemberVisibility visibility, bool isStatic) : base(name)
    {
        Type = type;
        Visibility = visibility;
        IsStatic = isStatic;
    }

    /// <summary>
    /// Used for method arguments.
    /// </summary>
    public VariableSymbol(string name, string type) : base(name)
    {
        Type = type;
        Visibility = MemberVisibility.Private;
        IsStatic = false;
    }
}