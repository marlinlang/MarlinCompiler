using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Class declaration.
/// </summary>
public class ClassTypeDefinitionNode : TypeDefinitionNode
{
    /// <summary>
    /// The type that this class inherits from, incl. module path 
    /// </summary>
    public string? BaseTypeFullName { get; set;  }

    public ClassTypeDefinitionNode(string name, GetAccessibility accessibility, string? baseTypeFullName)
        : base(name, accessibility)
    {
        BaseTypeFullName = baseTypeFullName;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.ClassDefinition(this);
    }
}