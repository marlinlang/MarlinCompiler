namespace MarlinCompiler.Common.Semantics.Symbols;

public sealed class StructTypeDeclarationSymbol : TypeDeclarationSymbol
{
    public GetAccessibility Accessibility { get; }

    public StructTypeDeclarationSymbol(string name, GetAccessibility accessibility) : base(name)
    {
        Accessibility = accessibility;
    }
}