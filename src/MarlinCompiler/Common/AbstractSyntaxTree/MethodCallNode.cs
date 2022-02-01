using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Method calls are both expressions and statements.
/// </summary>
public class MethodCallNode : IndexableExpressionNode
{
    public string MethodName { get; }
    public ExpressionNode[] Args { get; }

    public MethodCallNode(string methodName, ExpressionNode[] args)
    {
        MethodName = methodName;
        Args = args;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.MethodCall(this);
    }
}