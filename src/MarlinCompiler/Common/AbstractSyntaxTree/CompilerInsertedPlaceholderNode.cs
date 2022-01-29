using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// For chains of expressions, such as new app::Foo().X().Y, it's easier to
/// store each left side of the expression in a placeholder 'variable' (a special kind
/// of variable that can hold static type references too) and then operate on it. This
/// lets us have much simpler semantic analysis and checking when symbols start coming
/// into play in the intermediate phase.
/// </summary>
public class CompilerInsertedPlaceholderNode : Node
{
    /// <summary>
    /// The reference we're holding onto.
    /// </summary>
    public Node Reference { get; }

    public CompilerInsertedPlaceholderNode(Node reference)
    {
        Reference = reference;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor) => visitor.CompilerInsertedPlaceholder(this);
}