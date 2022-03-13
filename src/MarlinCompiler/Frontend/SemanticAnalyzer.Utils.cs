using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Frontend;

public sealed partial class SemanticAnalyzer
{
    /// <summary>
    /// The internal stack of scopes.
    /// </summary>
    private Stack<Scope> _scopes = new();

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
    /// Searches for a symbol.
    /// </summary>
    /// <param name="topmostScopeOnly">If true, only the topmost scope will be searched.</param>
    private Symbol? LookupSymbol(string name, bool topmostScopeOnly)
        => _scopes.Peek()!.LookupSymbol(name, topmostScopeOnly);

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
    /// Gets the type of an expression.
    /// </summary>
    private SemType GetExprType(ExpressionNode expr)
    {
        return expr switch
        {
            ArrayInitializerNode arrayInitializerNode => GetSemType(arrayInitializerNode.Type),
            NewClassInitializerNode newClassInitializerNode => GetSemType(newClassInitializerNode.Type),
            TypeReferenceNode typeReferenceNode => GetSemType(typeReferenceNode),
            IntegerNode integerNode => new SemType("std::Int32", null),
            
            BinaryOperatorNode binaryOperatorNode => throw new NotImplementedException(),
            MemberAccessNode memberAccessNode => throw new NotImplementedException(),
            MethodCallNode methodCallNode => throw new NotImplementedException(),
            VariableAssignmentNode variableAssignmentNode => throw new NotImplementedException(),
            
            _ => throw new ArgumentOutOfRangeException(nameof(expr))
        };
    }

    /// <summary>
    /// Checks if two types are compatible. 
    /// </summary>
    private bool DoTypesMatch(SemType expected, SemType given)
    {
        return expected.Name == given.Name;
    }
    
    /// <summary>
    /// Helper method.
    /// </summary>
    private SemType GetSemType(TypeReferenceNode node)
    {
        return new SemType(node.FullName, node.GenericTypeName);
    }
}