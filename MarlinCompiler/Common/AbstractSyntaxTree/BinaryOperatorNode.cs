using MarlinCompiler.Common.Visitors;
using MarlinCompiler.Frontend.Lexing;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Base binary operator class.
/// </summary>
public class BinaryOperatorNode : ExpressionNode
{
    public BinaryOperatorNode(TokenType op, Node left, Node right)
    {
        Operator = op;
        Left     = left;
        Right    = right;
    }

    public TokenType Operator { get; }
    public Node      Left     { get; set; }
    public Node      Right    { get; set; }

    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        throw new InvalidOperationException("Use subclasses");
    }
}