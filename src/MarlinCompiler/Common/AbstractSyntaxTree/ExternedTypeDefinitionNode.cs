using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Represents an externed type.
/// </summary>
public class ExternedTypeDefinitionNode : TypeDefinitionNode
{
    public string? LlvmTypeName { get; }
    
    public ExternedTypeDefinitionNode(string name, string module, GetAccessibility accessibility,
        string? llvmTypeName) : base(name, module, accessibility)
    {
        LlvmTypeName = llvmTypeName;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.ExternedTypeDefinition(this);
    }
}