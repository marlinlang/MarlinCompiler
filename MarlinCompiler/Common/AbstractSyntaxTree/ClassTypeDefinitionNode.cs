using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Class declaration.
/// </summary>
public class ClassTypeDefinitionNode : TypeDefinitionNode
{
    public ClassTypeDefinitionNode(
        string name,
        string module,
        GetAccessibility accessibility,
        bool isStatic,
        TypeReferenceNode? baseType,
        string? genericTypeParamName)
        : base(name, module, accessibility)
    {
        BaseType             = baseType;
        IsStatic             = isStatic;
        GenericTypeParamName = genericTypeParamName;
    }

    /// <summary>
    /// The type that this class inherits from, incl. module path 
    /// </summary>
    /// <remarks>Only null for the std.Object type.</remarks>
    public TypeReferenceNode? BaseType { get; set; }

    public bool IsStatic { get; }

    /// <summary>
    /// The type that is named by the generic type param
    /// </summary>
    public string? GenericTypeParamName { get; }

    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        return visitor.ClassDefinition(this);
    }
}