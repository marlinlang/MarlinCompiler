using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A reference to a type.
/// </summary>
public class TypeReferenceNode : IndexableExpressionNode
{
    public TypeReferenceNode(string fullName, TypeReferenceNode[] genericTypeArgs)
    {
        FullName             = fullName;
        GenericTypeArguments = genericTypeArgs;
    }

    /// <summary>
    /// The name of the type.
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// Generic type name (e.g. Array&lt;string&gt; - the string part)
    /// </summary>
    public TypeReferenceNode[] GenericTypeArguments { get; }

    public override string ToString()
    {
        // don't you love creating a whole list because someone created two ambiguous ext methods? I sure do! /s
        return GenericTypeArguments.Any()
                   ? $"{FullName}<{String.Join(", ", GenericTypeArguments.ToList())}>"
                   : FullName;
    }

    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        return visitor.TypeReference(this);
    }
}