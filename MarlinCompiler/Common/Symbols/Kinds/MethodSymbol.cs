using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a method symbol.
/// </summary>
public class MethodSymbol : NamedSymbol
{
    public MethodSymbol(MethodDeclarationNode node) : base(node.Name)
    {
        Accessibility = node.Accessibility;
        ReturnType    = null;
        IsStatic      = node.IsStatic;
        Parameters    = node.Parameters;
    }
    
    /// <summary>
    /// The accessibility of the method.
    /// </summary>
    public GetAccessibility Accessibility { get; }
    
    /// <summary>
    /// The return type of the method.
    /// </summary>
    /// <remarks>This is null by default. Semantic analysis is expected to fill it in.</remarks>
    public TypeUsageSymbol? ReturnType { get; set; }
    
    /// <summary>
    /// Whether the method is static.
    /// </summary>
    public bool IsStatic { get; }
    
    /// <summary>
    /// The parameters of the method.
    /// </summary>
    public VariableNode[] Parameters { get; }
}