using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a struct declaration.
/// </summary>
public class ExternTypeSymbol : TypeSymbol
{
    public ExternTypeSymbol(ExternTypeDefinitionNode node)
        : base(node.ModuleName, node.TypeName, node.Accessibility)
    {
        LlvmTypeName = node.LlvmTypeName;
    }
    
    /// <summary>
    /// The LLVM type name that is mapped onto this type.
    /// If this is null, the type is static.
    /// </summary>
    public string? LlvmTypeName { get; }
}