using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Expression.
/// </summary>
public class ExpressionNode : Node
{
    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        throw new InvalidOperationException("Cannot visit expression node directly, use subclasses.");
    }
}