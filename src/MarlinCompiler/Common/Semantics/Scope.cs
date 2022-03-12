namespace MarlinCompiler.Common.Semantics;

public class Scope
{
    public Scope? Parent { get; set; }
    public Symbol? ScopeInformation { get; set; }

    private readonly Dictionary<string, Symbol> _symbols;
    
    public Scope()
    {
        Parent = null;
        _symbols = new Dictionary<string, Symbol>();
    }

    public Scope(Scope parent)
    {
        Parent = parent;
        _symbols = new Dictionary<string, Symbol>();
    }
    
    /// <summary>
    /// Analogous to <see cref="Lookup" />
    /// </summary>
    /// <param name="name"></param>
    public Symbol? this[string name] => Lookup(name);

    /// <summary>
    /// Looks the name up in this and parent symbols.
    /// </summary>
    /// <param name="name">The name to look for.</param>
    /// <returns>The found symbol or null.</returns>
    public Symbol? Lookup(string name)
    {
        // lookup in current or parent scope
        return _symbols.ContainsKey(name)
            ? _symbols[name]
            : Parent?.Lookup(name);
    }

    /// <summary>
    /// Attempts adding a symbol to the current scope.
    /// </summary>
    /// <param name="symbol">The symbol to add.</param>
    /// <exception cref="SymbolAlreadyExistsException">Thrown if a symbol with that name already exists.</exception>
    public void Add(Symbol symbol)
    {
        string name = symbol.Name;
        
        try
        {
            _symbols.Add(name, symbol);
        }
        catch (ArgumentException)
        {
            throw new SymbolAlreadyExistsException(name, _symbols[name]);
        }
    }

    /// <summary>
    /// Adds the symbols from the other scope to this scope without removing them from the first one.
    /// </summary>
    public void AddFrom(Scope scope)
    {
        foreach (Symbol symbol in scope._symbols.Values)
        {
            Add(symbol);
        }
    }
}