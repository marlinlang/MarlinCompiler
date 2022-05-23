using System.Data;

namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a symbol for a type.
/// </summary>
public class TypeSymbol : NamedSymbol
{
    public static readonly TypeSymbol Void        = new("", "void", GetAccessibility.Public);
    public static readonly TypeSymbol UnknownType = new("???", "???", GetAccessibility.Public);

    protected TypeSymbol(string moduleName, string typeName, GetAccessibility accessibility)
        : base($"{moduleName}::{typeName}")
    {
        ModuleName      = moduleName;
        TypeName        = typeName;
        Accessibility   = accessibility;
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

    /// <summary>
    /// The symbol table for this type.
    /// </summary>
    public SymbolTable SymbolTable
    {
        get => _symbolTable ?? throw new NoNullAllowedException("Symbol table not initialized.");
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _symbolTable = value;
        }
    }

    /// <summary>
    /// Backing field for <see cref="SymbolTable"/>
    /// </summary>
    private SymbolTable? _symbolTable;
}