﻿namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a symbol for a type.
/// </summary>
public abstract class TypeSymbol : ISymbol
{
    protected TypeSymbol(string moduleName, string typeName, GetAccessibility accessibility)
    {
        ModuleName    = moduleName;
        TypeName      = typeName;
        Accessibility = accessibility;
    }

    /// <summary>
    /// The name of the module the class is contained within.
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// The name of the class.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// The accessibility of the class.
    /// </summary>
    public GetAccessibility Accessibility { get; }
}