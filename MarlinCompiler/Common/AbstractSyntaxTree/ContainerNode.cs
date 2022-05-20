using System.Collections;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A node that has children.
/// </summary>
public class ContainerNode : Node, IEnumerable<Node>
{
    public ContainerNode()
    {
        Children = new List<Node>();
    }

    /// <summary>
    /// The children of the node.
    /// </summary>
    public List<Node> Children { get; }

    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        foreach (Node node in Children)
        {
            visitor.Visit(node);
        }

        return default!;
    }

    public IEnumerator<Node> GetEnumerator()
    {
        return Children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Children.GetEnumerator();
    }
}