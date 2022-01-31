using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class IntegerNode : ExpressionNode
{
    public int Value { get; }

    public IntegerNode(int value)
    {
        Value = value;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.Integer(this);
    }
}