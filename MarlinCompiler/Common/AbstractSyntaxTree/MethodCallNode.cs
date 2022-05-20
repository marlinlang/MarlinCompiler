using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Method calls are both expressions and statements.
/// </summary>
public class MethodCallNode : IndexableExpressionNode
{
    public MethodCallNode(string methodName, bool isNativeCall, ExpressionNode[] arguments)
    {
        MethodName   = methodName;
        IsNativeCall = isNativeCall;
        Arguments    = arguments;
    }

    public string           MethodName { get; }
    public ExpressionNode[] Arguments  { get; }

    /// <summary>
    /// True for calls to LLVM functions instead of Marlin-defined ones
    /// </summary>
    public bool IsNativeCall { get; }

    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        return visitor.MethodCall(this);
    }
}