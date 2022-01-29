using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

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