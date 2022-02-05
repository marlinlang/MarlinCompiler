namespace MarlinCompiler.Common.Semantics.Symbols;

public sealed class ClassTypeDeclarationSymbol : TypeDeclarationSymbol
{
    public GetAccessibility Accessibility { get; }
    public bool IsStatic { get; }

    public ClassTypeDeclarationSymbol(string name, GetAccessibility accessibility, bool isStatic) : base(name)
    {
        Accessibility = accessibility;
        IsStatic = isStatic;
    }
}