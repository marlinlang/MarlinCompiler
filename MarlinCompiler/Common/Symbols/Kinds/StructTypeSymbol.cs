using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a struct declaration.
/// </summary>
public class StructTypeSymbol : TypeSymbol
{
    public StructTypeSymbol(StructTypeDefinitionNode node)
        : base(node.ModuleName, node.TypeName, node.Accessibility)
    {
    }
}