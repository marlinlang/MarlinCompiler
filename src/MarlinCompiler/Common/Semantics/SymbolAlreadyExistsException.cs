namespace MarlinCompiler.Common.Semantics;

public sealed class SymbolAlreadyExistsException : Exception
{
    public Symbol OtherSymbol { get; }
    
    public SymbolAlreadyExistsException(string symbolName, Symbol other) : base(symbolName)
    {
        OtherSymbol = other;
    }
}