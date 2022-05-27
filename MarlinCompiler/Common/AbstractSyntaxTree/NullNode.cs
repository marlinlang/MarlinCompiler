using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class NullNode : ExpressionNode
{
    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        return visitor.Null(this);
    }
}