namespace MarlinCompiler.Common.Semantics.Symbols;

public sealed class MethodDeclarationSymbol : Symbol
{
    public TypeReferenceSymbol? Type { get; }
    public GetAccessibility Accessibility { get; }
    public string Name { get; }
    public bool IsStatic { get; }

    public MethodDeclarationSymbol(TypeReferenceSymbol? type, GetAccessibility accessibility,
        string name, bool isStatic)
    {
        Type = type;
        Accessibility = accessibility;
        Name = name;
        IsStatic = isStatic;
    }
}