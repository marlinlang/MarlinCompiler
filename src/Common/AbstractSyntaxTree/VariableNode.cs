using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Base variable node. Used for arguments. Use inheritors for anything else.
/// </summary>
public class VariableNode : Node
{
    public VariableNode(TypeReferenceNode type, string name, ExpressionNode? value)
    {
        Type = type;
        Name = name;
        Value = value;
    }
    
    public TypeReferenceNode Type { get; }
    public string Name { get; }
    public ExpressionNode? Value { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        throw new InvalidOperationException("Cannot visit VariableNode directly");
    }
}