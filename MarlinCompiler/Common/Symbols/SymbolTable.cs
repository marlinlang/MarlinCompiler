using System.Collections.Immutable;
using System.Data;
using MarlinCompiler.Common.Symbols.Kinds;

namespace MarlinCompiler.Common.Symbols;

/// <summary>
/// Represents a symbol table. Tables are usually created for every scope the program has.
/// </summary>
public sealed class SymbolTable
{
    public SymbolTable(SymbolTable? parentTable, ISymbol? primarySymbol = null)
    {
        ParentTable          = parentTable;
        PrimarySymbol        = primarySymbol;
        _childTables         = new HashSet<SymbolTable>();
    }

    /// <summary>
    /// The primary symbol for this scope. This is used for scopes that are also symbols, e.g. methods or types.
    /// In these cases, this symbol will be the symbol of the method or type respectively. 
    /// </summary>
    /// <remarks>This is easier to implement than symbols having symbol tables.</remarks>
    public ISymbol? PrimarySymbol { get; }

    /// <summary>
    /// The parent table of this table, i.e. the parent scope.
    /// </summary>
    public SymbolTable? ParentTable { get; private set; }

    /// <summary>
    /// The children tables (scopes).
    /// </summary>
    private readonly HashSet<SymbolTable> _childTables;

    /// <summary>
    /// Looks up a symbol.
    /// </summary>
    /// <param name="predicate">The predicate to look with.</param>
    /// <typeparam name="TSymbol">The expected type of the symbol.</typeparam>
    /// <returns>The found symbol, never null.</returns>
    /// <exception cref="NoNullAllowedException">Thrown if the symbol does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown if the generic param <typeparamref name="TSymbol"/>
    /// doesn't match the type of the found symbol.</exception>
    public TSymbol LookupSymbol<TSymbol>(Predicate<ISymbol> predicate)
    {
        ISymbol? found = _childTables
                        .SingleOrDefault(
                             x => x.PrimarySymbol != null
                                  && predicate(x.PrimarySymbol)
                         )
                       ?.PrimarySymbol;

        if (found == default)
        {
            if (PrimarySymbol != default
                && predicate(PrimarySymbol))
            {
                found = PrimarySymbol;
            }
            else if (ParentTable != default)
            {
                found = ParentTable.LookupSymbol<ISymbol>(predicate);
            }
        }

        return found switch
        {
            TSymbol cast => cast,
            null         => throw new NoNullAllowedException("The symbol could not be found."),
            _            => throw new ArgumentException("The type of the found symbol does not match the expected type.")
        };
    }

    /// <summary>
    /// Calls <see cref="LookupSymbol{TSymbol}(System.Predicate{MarlinCompiler.Common.Symbols.ISymbol})"/> with
    /// a predicate for a named symbol of the same name.
    /// </summary>
    /// <param name="name">The name to look for.</param>
    /// <typeparam name="TSymbol">Expected symbol type.</typeparam>
    public TSymbol LookupSymbol<TSymbol>(string name) => LookupSymbol<TSymbol>(x => x is NamedSymbol named && named.Name == name);

    /// <summary>
    /// Attempts to lookup a symbol.
    /// </summary>
    /// <param name="predicate">The predicate to search for.</param>
    /// <param name="found">The found symbol.</param>
    /// <typeparam name="TSymbol">The symbol type to search for.</typeparam>
    /// <returns>Whether a matching symbol was found.</returns>
    public bool TryLookupSymbol<TSymbol>(Predicate<ISymbol> predicate, out TSymbol found)
    {
        try
        {
            found = LookupSymbol<TSymbol>(predicate);
            return true;
        }
        catch
        {
            found = default!;
            return false;
        }
    }

    /// <summary>
    /// Attempts to lookup a (named) symbol.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="found">The found symbol.</param>
    /// <typeparam name="TSymbol">The symbol type to search for.</typeparam>
    /// <returns>Whether a matching symbol was found.</returns>
    public bool TryLookupSymbol<TSymbol>(string name, out TSymbol found)
    {
        try
        {
            found = LookupSymbol<TSymbol>(name);
            return true;
        }
        catch
        {
            found = default!;
            return false;
        }
    }

    /// <summary>
    /// Adds a symbol to this symbol table (scope).
    /// </summary>
    /// <param name="symbol">The symbol to add.</param>
    /// <exception cref="SymbolNameAlreadyExistsException">Thrown if the name of the symbol is taken.</exception>
    /// <remarks>Creates a new symbol table under the hood
    /// and stores the symbol as the <see cref="PrimarySymbol"/>.</remarks>
    public void AddSymbol(ISymbol symbol)
    {
        if (symbol is NamedSymbol namedSymbol
            && _childTables.Any(
                x => x.PrimarySymbol is NamedSymbol checkNamedSymbol && checkNamedSymbol.Name == namedSymbol.Name
            ))
        {
            throw new SymbolNameAlreadyExistsException(namedSymbol.Name);
        }

        _childTables.Add(new SymbolTable(this, symbol));
    }

    /// <summary>
    /// Adds a child scope under this symbol table.
    /// </summary>
    /// <param name="symbolTable">The symbol table to add.</param>
    /// <exception cref="SymbolNameAlreadyExistsException">Thrown if the name of the symbol is taken.</exception>
    /// <remarks>This will modify the parent table of the added table to the current one.</remarks>
    public void AddSymbol(SymbolTable symbolTable)
    {
        if (symbolTable.PrimarySymbol is NamedSymbol namedSymbol
            && _childTables.Any(
                x => x.PrimarySymbol is NamedSymbol checkNamedSymbol && checkNamedSymbol.Name == namedSymbol.Name
            ))
        {
            throw new SymbolNameAlreadyExistsException(namedSymbol.Name);
        }

        symbolTable.ParentTable = this;
        _childTables.Add(symbolTable);
    }

    /// <summary>
    /// Takes ownership of all symbols from the other table.
    /// </summary>
    /// <param name="other">The table to work with.</param>
    public void TakeSymbolsFrom(SymbolTable other)
    {
        foreach (SymbolTable child in other._childTables.ToImmutableArray())
        {
            other._childTables.Remove(child);
            AddSymbol(child);
        }
    }
}