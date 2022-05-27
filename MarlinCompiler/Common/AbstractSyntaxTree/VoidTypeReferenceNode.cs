namespace MarlinCompiler.Common.AbstractSyntaxTree;

public sealed class VoidTypeReferenceNode : TypeReferenceNode
{
    public VoidTypeReferenceNode() : base("void", false, Array.Empty<TypeReferenceNode>())
    {
        // We must expose a constructor, because nodes have different FileLocations attached to them
    }
}