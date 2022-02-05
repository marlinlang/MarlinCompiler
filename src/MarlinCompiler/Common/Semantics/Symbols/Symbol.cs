namespace MarlinCompiler.Common.Semantics.Symbols;

/// <summary>
/// Represents a symbol.
/// </summary>
public class Symbol
{
    public Symbol? Parent { get; private set; }
    public List<Symbol> Children { get; }

    public Symbol()
    {
        Children = new List<Symbol>();
    }

    /// <summary>
    /// Adds a child symbol to this one.
    /// </summary>
    public void AddChild(Symbol symbol)
    {
        if (symbol == null)
        {
            throw new ArgumentNullException("Child node cannot be null");
        }
        
        symbol.Parent = this;
        
        if (Children.Contains(symbol)) return;
        
        Children.Add(symbol);
    }

    /// <summary>
    /// Searches in this symbol and parent symbols by the given predicate.
    /// </summary>
    public Symbol? Search(Predicate<Symbol> predicate)
    {
        if (predicate(this)) return this;

        Symbol? child = Children.Find(predicate);
        if (child != default)
        {
            return child;
        }

        return Parent?.Search(predicate);
    }

    /// <summary>
    /// Analogical to Search(), but it doesn't look in the children
    /// </summary>
    public Symbol? FindParent(Predicate<Symbol> predicate)
    {
        return predicate(this)
            ? this
            : Parent?.Search(predicate);
    }
}