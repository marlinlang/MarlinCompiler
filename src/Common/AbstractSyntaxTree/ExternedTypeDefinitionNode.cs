using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Represents an externed type.
/// </summary>
public class ExternedTypeDefinitionNode : TypeDefinitionNode
{
    public ExternedTypeDefinitionNode(string name, string module, GetAccessibility accessibility,
        bool isStatic, string? llvmTypeName) : base(name, module, accessibility)
    {
        LlvmTypeName = llvmTypeName;
        IsStatic = isStatic;
    }
    
    public string? LlvmTypeName { get; }
    public bool IsStatic { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.ExternedTypeDefinition(this);
    }
}