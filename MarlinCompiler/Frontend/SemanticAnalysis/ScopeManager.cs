using MarlinCompiler.Common.Symbols;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

/// <summary>
/// Handles scopes. This is essentially a wrapper around a stack of symbol tables.
/// </summary>
public sealed class ScopeManager
{
    public ScopeManager()
    {
        _scopeStack = new Stack<SymbolTable>();
    }
    
    private Stack<SymbolTable> _scopeStack;
    
    public SymbolTable CurrentScope => _scopeStack.Peek();
    
    public void PushScope(SymbolTable scope)
    {
        _scopeStack.Push(scope);
    }
    
    public void PopScope()
    {
        _scopeStack.Pop();
    }
}