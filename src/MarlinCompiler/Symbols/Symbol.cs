namespace MarlinCompiler.Symbols;

public class Symbol
{
    public string Name { get; }
    
    /// <summary>
    /// A user-friendly string that describes what the symbol represents, e.g. a variable.
    /// </summary>
    public virtual string UserType => "???";

    public Symbol? Parent { get; private set; }
    public List<Symbol> Scope { get; }
    
    private string Guid { get; }

    public object? CustomTargetData { get; set; }
    
    public static IEqualityComparer<Symbol> GuidComparer { get; } = new GuidEqualityComparer();
    
    public Symbol(string name)
    {
        Name = name;
        Scope = new List<Symbol>();
        Guid = System.Guid.NewGuid().ToString();
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
    
    public Symbol[] LookupMultiple(string name)
    {
        List<Symbol> found = new();

        if (Name == name)
        {
            found.Add(this);
        }
        
        foreach (Symbol sym in Scope)
        {
            if (sym.Name == name)
            {
                found.Add(sym);
            }
        }

        if (Parent != null)
        {
            found.AddRange(Parent.LookupMultiple(name));
        }
        
        return found.Distinct().ToArray();
    }

    public void AddChild(Symbol sym)
    {
        Scope.Add(sym);
        sym.Parent = this;
    }

    private sealed class GuidEqualityComparer : IEqualityComparer<Symbol>
    {
        public bool Equals(Symbol? x, Symbol? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Guid == y.Guid;
        }

        public int GetHashCode(Symbol obj)
        {
            return obj.Guid.GetHashCode();
        }
    }

    public override int GetHashCode() => Guid.GetHashCode();

    public string GetPath()
    {
        return Parent != null && Parent is not RootSymbol
            ? Parent.GetPath() + "." + Name
            : Name;
    }
}