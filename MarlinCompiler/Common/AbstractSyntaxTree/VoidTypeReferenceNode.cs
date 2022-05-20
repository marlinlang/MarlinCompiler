namespace MarlinCompiler.Common.AbstractSyntaxTree;

public sealed class VoidTypeReferenceNode : TypeReferenceNode
{
    public VoidTypeReferenceNode() : base("void")
    {
        // We must expose a constructor, because nodes have different FileLocations attached to them
    }
}