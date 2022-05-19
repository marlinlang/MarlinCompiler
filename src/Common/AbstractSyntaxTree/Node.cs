using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// The base class for all nodes.
/// </summary>
public abstract class Node
{
    /// <summary>
    /// The location of the source representation of the node.
    /// </summary>
    public FileLocation? Location { get; init; }

    /// <summary>
    /// The metadata attached to this node.
    /// </summary>
    public INodeMetadata? Metadata { get; set; }

    /// <summary>
    /// Routes a visitor to the correct method on itself for this node type.
    /// </summary>
    public abstract T AcceptVisitor<T>(IAstVisitor<T> visitor);
}