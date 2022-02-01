using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A reference to a type.
/// </summary>
public sealed class TypeReferenceNode : IndexableExpressionNode
{
    public string FullName { get; set; }
    public TypeReferenceNode? GenericType { get; set; }

    public TypeReferenceNode(string fullName, TypeReferenceNode? genericType = null)
    {
        FullName = fullName;
        GenericType = genericType;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.TypeReference(this);
    }
}