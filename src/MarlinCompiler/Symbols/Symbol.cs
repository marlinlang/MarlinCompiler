namespace MarlinCompiler.Symbols;

public class Symbol
{
    public string Name { get; }

    public Symbol Parent { get; private set; }
    private List<Symbol> Scope { get; }
    
    public Symbol(string name)
    {
        Name = name;
        Scope = new List<Symbol>();
    }

    public Symbol Lookup(string name)
    {
        if (Name == name)
        {
            return this;
        }
    
        foreach (Symbol sym in Scope)
        {
            if (sym.Name == name)
            {
                return sym;
            }
        }

        return Parent?.Lookup(name);
    }

    public void AddChild(Symbol sym)
    {
        Scope.Add(sym);
        sym.Parent = this;
    }
}