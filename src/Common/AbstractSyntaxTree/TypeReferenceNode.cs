using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A reference to a type.
/// </summary>
public sealed class TypeReferenceNode : IndexableExpressionNode
{
    public TypeReferenceNode(string fullName, TypeReferenceNode? genericTypeName = null)
    {
        FullName = fullName;
        GenericTypeName = genericTypeName;
    }
    
    /// <summary>
    /// The name of the type.
    /// </summary>
    public string FullName { get; }
    
    /// <summary>
    /// Generic type name (e.g. Array&lt;string&gt; - the string part)
    /// </summary>
    public TypeReferenceNode? GenericTypeName { get; }

    public override string ToString()
    {
        return GenericTypeName != null
            ? $"{FullName}<{GenericTypeName}>"
            : FullName;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.TypeReference(this);
    }
}