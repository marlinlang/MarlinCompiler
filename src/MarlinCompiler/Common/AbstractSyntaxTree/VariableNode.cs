using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Base variable node. Used for arguments. Use inheritors for anything else.
/// </summary>
public class VariableNode : Node
{
    public TypeReferenceNode Type { get; }
    public string Name { get; }
    public Node Value { get; }

    public VariableNode(TypeReferenceNode type, string name, Node value)
    {
        Type = type;
        Name = name;
        Value = value;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        throw new InvalidOperationException("Cannot visit VariableNode directly");
    }
}