using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class TupleNode : ExpressionNode
{
    public ExpressionNode[] Values { get; }

    public TupleNode(ExpressionNode[] values)
    {
        Values = values;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.Tuple(this);
    }
}