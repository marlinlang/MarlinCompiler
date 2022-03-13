using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A reference to a type.
/// </summary>
public sealed class TypeReferenceNode : IndexableExpressionNode
{
    public TypeReferenceNode(string fullName, string? genericTypeName = null)
    {
        FullName = fullName;
        GenericTypeName = genericTypeName;
    }
    
    /// <summary>
    /// The name of the type.
    /// </summary>
    public string FullName { get; }
    
    /// <summary>
    /// Generic type name (e.g. Array<string> - the string part)
    /// </summary>
    public string? GenericTypeName { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.TypeReference(this);
    }
}