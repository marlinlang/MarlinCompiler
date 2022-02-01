using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Class declaration.
/// </summary>
public class StructTypeDefinitionNode : TypeDefinitionNode
{
    public StructTypeDefinitionNode(string name, string module, GetAccessibility accessibility)
        : base(name, module, accessibility)
    {
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.StructDefinition(this);
    }
}