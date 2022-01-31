using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class VariableAssignmentNode : IndexableExpressionNode
{
    public string Name { get; }
    public ExpressionNode Value { get; }

    public VariableAssignmentNode(string name, ExpressionNode value)
    {
        Name = name;
        Value = value;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.VariableAssignment(this);
    }
}