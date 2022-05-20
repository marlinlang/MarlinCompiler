using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

public class VariableSymbol : ISymbol
{
    public VariableSymbol(VariableNode node)
    {
        Name = node.Name;
        Type = null;
    }

    /// <summary>
    /// The name of the property.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The type of the property.
    /// </summary>
    /// <remarks>Null by default.</remarks>
    public TypeUsageSymbol? Type { get; }
}