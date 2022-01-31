using MarlinCompiler.Common.Visitors;
using MarlinCompiler.Frontend;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Base binary operator class.
/// </summary>
public class BinaryOperatorNode : ExpressionNode
{
    public TokenType Operator { get; }
    public Node Left { get; set; }
    public Node Right { get; set; }

    public BinaryOperatorNode(TokenType op, Node left, Node right)
    {
        Operator = op;
        Left = left;
        Right = right;
    }
    
    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        throw new InvalidOperationException("Use subclasses");
    }
}