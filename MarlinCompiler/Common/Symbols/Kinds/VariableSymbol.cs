using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

public class VariableSymbol : NamedSymbol
{
    public static readonly VariableSymbol UnknownVariable = new(new VariableNode(null!, "<unknown>", null));
    
    public VariableSymbol(VariableNode node) : base(node.Name)
    {
        Type          = TypeUsageSymbol.UnknownType;
        IsInitialized = true;
    }

    /// <summary>
    /// The type of the property.
    /// </summary>
    public TypeUsageSymbol Type { get; set; }

    /// <summary>
    /// For local variables, this should be false, until a value is assigned.
    /// </summary>
    public bool IsInitialized { get; set; }
}