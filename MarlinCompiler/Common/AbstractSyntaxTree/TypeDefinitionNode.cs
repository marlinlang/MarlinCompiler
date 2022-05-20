using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A type definition. Do not use directly - use inheritors.
/// </summary>
public class TypeDefinitionNode : ContainerNode
{
    public TypeDefinitionNode(string name, string module, GetAccessibility accessibility)
    {
        TypeName     = name;
        ModuleName    = module;
        Accessibility = accessibility;
    }

    /// <summary>
    /// The name of the type without the module path.
    /// E.g., Console instead of std::Console
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// The name of the module the type is located within.
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// The accessibility of the type.
    /// </summary>
    public GetAccessibility Accessibility { get; }

    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        throw new InvalidOperationException($"Cannot visit TypeDefinitions directly. Use subclasses.");
    }
}