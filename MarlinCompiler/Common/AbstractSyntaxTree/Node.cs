using System.Data;
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
    /// Whether the node has metadata.
    /// </summary>
    public bool HasMetadata => _metadata != null;

    /// <summary>
    /// The metadata for the node.
    /// </summary>
    private object? _metadata;

    /// <summary>
    /// Accesses the metadata for a node.
    /// </summary>
    /// <typeparam name="TMetadata">The expected type of the metadata.</typeparam>
    /// <returns>The metadata, always a non-null value.</returns>
    /// <exception cref="NoNullAllowedException">Thrown if the stored metadata is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the generic param <typeparamref name="TMetadata"/>
    /// doesn't match the type of the stored metadata.</exception>
    public TMetadata GetMetadata<TMetadata>() => _metadata switch
    {
        null => throw new NoNullAllowedException("The metadata for this node is empty."),
        TMetadata metadata => metadata,
        _ => throw new ArgumentException("Generic type passed to GetMetadata does not match the actual type of the metadata. "
                                         + $"Expected {typeof(TMetadata).Name}, got {_metadata.GetType().Name}.")
    };

    /// <summary>
    /// Checks whether the node metadata is of the specified type.
    /// </summary>
    /// <exception cref="NoNullAllowedException">Metadata is not set.</exception>
    public bool MetadataIs<TMetadata>()
        => _metadata != null ? _metadata is TMetadata : throw new NoNullAllowedException("Metadata is not set.");

    public void SetMetadata(object metadata) => _metadata = metadata;

    /// <summary>
    /// Routes a visitor to the correct method on itself for this node type.
    /// </summary>
    public abstract T AcceptVisitor<T>(AstVisitor<T> visitor);
}