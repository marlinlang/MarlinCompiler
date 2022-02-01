﻿namespace MarlinCompiler.Common.Symbols;

/// <summary>
/// Representation of a class in the symbol table.
/// </summary>
public class ClassTypeSymbol : TypeSymbol
{
    /// <summary>
    /// The base class of this class.
    /// </summary>
    public string? BaseClass { get; init; }
    
    public ClassTypeSymbol(string name, string module, GetAccessibility accessibility)
        : base(name, module, accessibility)
    {
    }
}