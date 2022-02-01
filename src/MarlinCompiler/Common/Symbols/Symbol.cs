using System.Collections;

namespace MarlinCompiler.Common.Symbols;

public class Symbol : IEnumerable<Symbol>
{
    /// <summary>
    /// The parent of this symbol.
    /// </summary>
    public Symbol? Parent { get; private set; }
    
    /// <summary>
    /// The name of this symbol.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Custom data.
    /// </summary>
    public object? Tag { get; set; }

    private readonly List<Symbol> _children;

    protected Symbol(string name)
    {
        Parent = null;
        Name = name;
        Tag = null;
        _children = new List<Symbol>();
    }

    /// <summary>
    /// Adds a child to this symbol's scope and sets the child's Parent property to this symbol.
    /// </summary>
    public void AddChild(Symbol symbol)
    {
        symbol.Parent = this;
        _children.Add(symbol);
    }

    /// <summary>
    /// Searched for a symbol in the children of this symbol by name. Does not recurse.
    /// </summary>
    public Symbol? FindInChildren(string name)
    {
        return _children.Find(x => x.Name == name);
    }

    /// <summary>
    /// Searches for a name in this and parent symbols.
    /// </summary>
    /// <param name="predicate">Predicate to use for searching.</param>
    public Symbol? Lookup(Predicate<Symbol> predicate)
    {
        // obviously first check if we should cover that
        if (predicate(this)) return this;
        
        // check if any child matches the predicate
        Symbol? child = _children.Find(predicate);
        if (child != default)
        {
            return child;
        }

        // no? well, ask the parent if there is one or return default
        return Parent?.Lookup(predicate);
    }

    public IEnumerator<Symbol> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();
}