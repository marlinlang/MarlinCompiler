using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

public class VariableSymbol : NamedSymbol
{
    public VariableSymbol(VariableNode node) : base(node.Name)
    {
        Type = null;
    }

    /// <summary>
    /// The type of the property.
    /// </summary>
    /// <remarks>Null by default.</remarks>
    public TypeUsageSymbol? Type { get; }
}