using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class ReturnStatementNode : Node
{
    public ReturnStatementNode(ExpressionNode? value)
    {
        Value = value;
    }
    
    public ExpressionNode? Value { get; }
    
    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        return visitor.ReturnStatement(this);
    }
}