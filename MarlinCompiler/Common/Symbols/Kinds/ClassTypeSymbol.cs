using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a symbol for a class.
/// </summary>
public sealed class ClassTypeSymbol : TypeSymbol
{
    public ClassTypeSymbol(ClassTypeDefinitionNode node)
        : base(node.ModuleName, node.ClassName, node.Accessibility)
    {
        GenericParamName = node.GenericTypeParamName;
        IsStatic         = node.IsStatic;
    }
    
    /// <summary>
    /// The generic parameter, if one exists.
    /// </summary>
    public string? GenericParamName { get; }
    
    /// <summary>
    /// Whether the class is static or not.
    /// </summary>
    public bool IsStatic { get; }
    
    /// <summary>
    /// The type that this class inherits. 
    /// </summary>
    /// <remarks>Null by default. Assign by hand.</remarks>
    public TypeSymbol? BaseType { get; set; }
}