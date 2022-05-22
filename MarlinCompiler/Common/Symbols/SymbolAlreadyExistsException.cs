using MarlinCompiler.Frontend.Parsing;

namespace MarlinCompiler.Common.Symbols;

/// <summary>
/// Thrown when a symbol name already exists in a table.
/// </summary>
public class SymbolNameAlreadyExistsException : Exception
{
    public SymbolNameAlreadyExistsException(string symbolName)
        : base($"The symbol {symbolName} already exists in the current scope.")
    {
    }
}