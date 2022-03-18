using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class IntegerNode : ExpressionNode
{
    public IntegerNode(int value)
    {
        Value = value;
    }
    
    public int Value { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.Integer(this);
    }
}