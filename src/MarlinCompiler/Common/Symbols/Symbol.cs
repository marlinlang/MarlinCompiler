﻿using System.Collections;

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
    
    /// <summary>
    /// ID of the symbol. Used to prevent duplicate symbols under one parent.
    /// </summary>
    private string _id = Guid.NewGuid().ToString();

    /// <summary>
    /// The children of the symbol.
    /// </summary>
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
        if (_children.Find(x => x._id == symbol._id) != default)
        {
            return;
        }
        
        symbol.Parent = this;
        _children.Add(symbol);
    }

    /// <summary>
    /// Searched for a symbol in the children of this symbol by name. Does not recurse.
    /// </summary>
    public virtual Symbol? FindInChildren(string name)
    {
        return _children.Find(x => x.Name == name);
    }

    /// <summary>
    /// Searches for a symbol in this and parent symbols.
    /// </summary>
    /// <param name="predicate">Predicate to use for searching.</param>
    public virtual Symbol? Find(Predicate<Symbol> predicate)
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
        return Parent?.Find(predicate);
    }

    /// <summary>
    /// Searches for many symbols in this and parent symbols.
    /// </summary>
    /// <param name="predicate">Predicate to use for searching.</param>
    public virtual Symbol[] FindAll(Predicate<Symbol> predicate)
    {
        List<Symbol> found = new();

        // obviously first check if we should cover that
        if (predicate(this)) found.Add(this);
        
        // check for matching children
        found.AddRange(_children.FindAll(predicate));

        // ask the parent if there is one
        if (Parent != default) found.AddRange(Parent.FindAll(predicate)); 

        return found.Distinct().ToArray();
    }

    public IEnumerator<Symbol> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();

    public override bool Equals(object? obj) => obj is Symbol s && s._id == _id;
    public override int GetHashCode() => _id.GetHashCode();
}