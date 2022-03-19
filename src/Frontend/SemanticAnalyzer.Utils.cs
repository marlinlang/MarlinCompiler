using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Frontend;

public sealed partial class SemanticAnalyzer
{
    /// <summary>
    /// The internal stack of scopes.
    /// </summary>
    private readonly Stack<Scope> _scopes = new();

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
    private (bool compatible, string expectedFullName, string givenFullName) AreTypesCompatible(SemType expected, SemType given)
    {
        expected = expected.AttemptResolveGeneric();
        given = given.AttemptResolveGeneric();
        
        if (expected.Name != given.Name)
        {
            // Obvious: name mismatch!
            // TODO: Inheritance
            return (false, expected.ToString(), given.ToString());
        }

        if (expected.GenericTypeParameter is null != given.GenericTypeParameter is null)
        {
            // One type has generic param, but the other doesn't
            return (false, expected.ToString(), given.ToString());
        }
        
        if (expected.GenericTypeParameter == null)
        {
            // No generic param/arg in either
            return (true, expected.ToString(), given.ToString());
        }
        
        // Both have generic args, make sure they're compatible
        (bool compatible, string _, string _) = AreTypesCompatible(
            expected.GenericTypeParameter!,
            given.GenericTypeParameter!
        );
        return (compatible, expected.ToString(), given.ToString());
    }

    /// <summary>
    /// Used for visiting a type reference with a generic parameter accurately.
    /// </summary>
    private void VisitTypeReferenceGeneric(TypeReferenceNode node)
    {
        if (node.GenericTypeName is null)
        {
            throw new InvalidOperationException(
                $"Cannot use {nameof(VisitTypeReferenceGeneric)} for non-generic types");
        }

        
        
        throw new NotImplementedException();
        
        /*
         * if (node.GenericTypeName != null)
            {
                Visit(node.GenericTypeName);

                Symbol? symbol = (node.GenericTypeName.Metadata as SymbolMetadata)?.Symbol;
                if (symbol != null && symbol != Symbol.UnknownType && symbol.Kind != SymbolKind.GenericTypeParam)
                {
                    type.Scope.SetGenericArg(0, symbol.Type);
                }
            }
         */
    }
    
    /// <summary>
    /// Helper method.
    /// </summary>
    private static SemType GetSemType(TypeReferenceNode node)
    {
        return new SemType(
            node.FullName,
            node.GenericTypeName != null
                ? GetSemType(node.GenericTypeName)
                : null
            );
    }
}