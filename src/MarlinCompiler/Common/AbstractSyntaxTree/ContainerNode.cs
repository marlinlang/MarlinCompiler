using System.Collections;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A node that has children.
/// </summary>
public class ContainerNode : Node, IEnumerable<Node>
{
    /// <summary>
    /// The children of the node.
    /// </summary>
    public List<Node> Children { get; }

    public ContainerNode()
    {
        Children = new List<Node>();
    }
    
    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        foreach (Node node in Children)
        {
            visitor.Visit(node);
        }

        return default;
    }

    public IEnumerator<Node> GetEnumerator() => Children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Children.GetEnumerator();
}