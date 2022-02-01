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
    public string? BaseType { get; set;  }
    
    public ClassTypeDefinitionNode(string name, string module, GetAccessibility accessibility, string? baseType)
        : base(name, module, accessibility)
    {
        BaseType = baseType;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.ClassDefinition(this);
    }
}