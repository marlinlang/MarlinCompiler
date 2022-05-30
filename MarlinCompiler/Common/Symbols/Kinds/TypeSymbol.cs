using System.Data;
using System.Diagnostics.CodeAnalysis;
using Ubiquity.NET.Llvm.Types;

namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a symbol for a type.
/// </summary>
public class TypeSymbol : NamedSymbol
{
    public static readonly TypeSymbol Void        = new(String.Empty, "void", GetAccessibility.Public);
    public static readonly TypeSymbol Null        = new(String.Empty, "nullptr", GetAccessibility.Public);
    public static readonly TypeSymbol UnknownType = new(String.Empty, "???", GetAccessibility.Public);

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

    public override string Name => GetStringRepresentation();

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

    /// <summary>
    /// Creates a string representation of the type.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public string GetStringRepresentation()
    {
        if (this == Null)
        {
            return "null";
        }
        if (this == Void)
        {
            return "void";
        }
        if (this == UnknownType)
        {
            return "<unknown type>";
        }

        return $"{ModuleName}::{TypeName}";
    }

    /// <summary>
    /// The LLVM type reference for this type.
    /// </summary>
    public ITypeRef? LlvmTypeRef { get; set; }
}