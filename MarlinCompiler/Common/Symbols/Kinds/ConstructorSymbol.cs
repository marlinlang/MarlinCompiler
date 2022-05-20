using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

public class ConstructorSymbol : ISymbol
{
    public ConstructorSymbol(ConstructorDeclarationNode node)
    {
        Accessibility = node.Accessibility;
        Parameters    = node.Parameters;
    }

    /// <summary>
    /// The visibility of this constructor.
    /// </summary>
    public GetAccessibility Accessibility { get; }

    /// <summary>
    /// The parameters that this constructor takes.
    /// </summary>
    public VariableNode[] Parameters { get; }
}