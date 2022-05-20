using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Represents an empty statement (a single semicolon).
/// Does not get visited by visitors.
/// </summary>
public class EmptyStatementNode : Node
{
    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        return default!;
    }
}