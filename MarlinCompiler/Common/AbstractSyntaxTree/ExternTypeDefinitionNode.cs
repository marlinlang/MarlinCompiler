using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Represents an externed type.
/// </summary>
public class ExternTypeDefinitionNode : TypeDefinitionNode
{
    public ExternTypeDefinitionNode(
        string name,
        string module,
        GetAccessibility accessibility,
        bool isStatic,
        string? llvmTypeName) : base(name, module, accessibility)
    {
        LlvmTypeName = llvmTypeName;
        IsStatic     = isStatic;
    }

    public string? LlvmTypeName { get; }
    public bool    IsStatic     { get; }

    public override T AcceptVisitor<T>(AstVisitor<T> visitor)
    {
        return visitor.ExternTypeDefinition(this);
    }
}