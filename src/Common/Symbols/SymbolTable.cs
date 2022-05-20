﻿using System.CommandLine.Help;
using System.Data;

namespace MarlinCompiler.Common.Symbols;

/// <summary>
/// Represents a symbol table. Tables are usually created for every scope the program has.
/// </summary>
public sealed class SymbolTable
{
    public SymbolTable(ISymbol? primarySymbol = null)
    {
        PrimarySymbol = primarySymbol;
        _childTables  = new HashSet<SymbolTable>();
    }

    /// <summary>
    /// The primary symbol for this scope. This is used for scopes that are also symbols, e.g. methods or types.
    /// In these cases, this symbol will be the symbol of the method or type respectively. 
    /// </summary>
    /// <remarks>This is easier to implement than symbols having symbol tables.</remarks>
    public ISymbol? PrimarySymbol { get; set; }

    /// <summary>
    /// The parent table of this table, i.e. the parent scope.
    /// </summary>
    public SymbolTable? ParentTable { get; set; }

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
    /// <exception cref="NoNullAllowedException">Thrown if the symbol does not exist..</exception>
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

        if (PrimarySymbol != default
            && predicate(PrimarySymbol))
        {
            found = PrimarySymbol;
        }
        else if (ParentTable != default)
        {
            found = ParentTable.LookupSymbol<ISymbol>(predicate);
        }

        return found switch
        {
            TSymbol cast => cast,
            null         => throw new NoNullAllowedException("The symbol could not be found."),
            _            => throw new ArgumentException("The type of the found symbol does not match the expected type.")
        };
    }

    /// <summary>
    /// Looks up symbols based on a hash code. This is faster than
    /// <see cref="LookupSymbol{TSymbol}(System.Predicate{MarlinCompiler.Common.Symbols.ISymbol})"/>, but only works if you
    /// know the hashcode that you will be working with.
    /// </summary>
    /// <param name="hashCode">The hash code to search for.</param>
    /// <typeparam name="TSymbol">The expected type of the symbol.</typeparam>
    /// <returns>The found symbol, never null.</returns>
    /// <exception cref="NoNullAllowedException">Thrown if the symbol does not exist..</exception>
    /// <exception cref="ArgumentException">Thrown if the generic param <typeparamref name="TSymbol"/>
    /// doesn't match the type of the found symbol.</exception>
    public TSymbol LookupSymbol<TSymbol>(int hashCode)
    {
        ISymbol? found = _childTables
                        .SingleOrDefault(
                             x => x.PrimarySymbol                  != null
                                  && x.PrimarySymbol.GetHashCode() == hashCode
                         )
                       ?.PrimarySymbol;

        if (PrimarySymbol                  != default
            && PrimarySymbol.GetHashCode() == hashCode)
        {
            found = PrimarySymbol;
        }
        else if (ParentTable != default)
        {
            found = ParentTable.LookupSymbol<ISymbol>(hashCode);
        }

        return found switch
        {
            TSymbol cast => cast,
            null         => throw new NoNullAllowedException("The symbol could not be found."),
            _            => throw new ArgumentException("The type of the found symbol does not match the expected type.")
        };
    }

    /// <summary>
    /// Adds a symbol to this symbol table (scope).
    /// </summary>
    /// <param name="symbol">The symbol to add.</param>
    /// <remarks>Creates a new symbol table under the hood
    /// and stores the symbol as the <see cref="PrimarySymbol"/>.</remarks>
    public void AddSymbol(ISymbol symbol)
    {
        _childTables.Add(new SymbolTable(symbol));
    }

    /// <summary>
    /// Adds a child scope under this symbol table.
    /// </summary>
    /// <param name="symbolTable">The symbol table to add.</param>
    public void AddSymbol(SymbolTable symbolTable)
    {
        _childTables.Add(symbolTable);
    }
}