using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A type definition. Do not use directly - use inheritors.
/// </summary>
public class TypeDefinitionNode : ContainerNode
{
    /// <summary>
    /// The name of the type without the module path.
    /// E.g., Console instead of std::Console
    /// </summary>
    public string LocalName { get; }

    /// <summary>
    /// The accessibility of the type.
    /// </summary>
    public Accessibility Accessibility { get; }

    public TypeDefinitionNode(string name, Accessibility accessibility)
    {
        LocalName = name;
        Accessibility = accessibility;

        if (!accessibility.HasFlag(Accessibility.NoModify))
        {
            throw new InvalidOperationException("You must specify the NoModify accessibility flag for types.");
        }
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
        => throw new InvalidOperationException($"Cannot visit TypeDefinitions directly. Use subclasses.");
    
}