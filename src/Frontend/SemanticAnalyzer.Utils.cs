using System.Data;
using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Frontend;

public sealed partial class SemanticAnalyzer
{
    /// <summary>
    /// The internal stack of scopes.
    /// </summary>
    private Stack<Scope> _scopes = new();

    private Scope CurrentScope => _scopes.Peek();
    
    /// <summary>
    /// Adds a new scope on top.
    /// </summary>
    private Scope PushScope()
    {
        Scope scope = new(_scopes.Count == 0 ? null : _scopes.Peek());
        _scopes.Push(scope);
        return scope;
    }

    /// <summary>
    /// Adds the given scope to the scope stack without changing the scope itself.
    /// </summary>
    private void UseScope(Scope scope) => _scopes.Push(scope);
    
    /// <summary>
    /// Removes the topmost scope.
    /// </summary>
    private void PopScope() => _scopes.Pop();

    /// <summary>
    /// Adds a symbol to the topmost scope. 
    /// </summary>
    private void AddSymbolToScope(Symbol symbol)
    {
        _scopes.Peek().AddSymbol(symbol);
    }
    
    /// <summary>
    /// Adds the symbol in the metadata to the topmost scope. 
    /// </summary>
    private void AddSymbolToScope(SymbolMetadata metadata)
    {
        if (metadata.Symbol == null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }

        _scopes.Peek().AddSymbol(metadata.Symbol);
    }

    /// <summary>
    /// Checks whether two types are compatible. Supports generics.
    /// </summary>
    private bool AreTypesCompatible(ref SemType expected, SemType given)
    {
        // TODO: Inheritance
        // Make it work with both the base types but with the generic args as well

        if (expected.Name != given.Name)
        {
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// Helper method.
    /// </summary>
    private SemType GetSemType(TypeReferenceNode node)
    {
        return new SemType(node.FullName, node.GenericTypeName != null ? GetSemType(node.GenericTypeName) : null);
    }
}