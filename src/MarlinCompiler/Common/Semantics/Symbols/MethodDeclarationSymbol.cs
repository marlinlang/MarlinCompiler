namespace MarlinCompiler.Common.Semantics.Symbols;

public sealed class MethodDeclarationSymbol : Symbol
{
    public TypeReferenceSymbol? Type { get; }
    public GetAccessibility Accessibility { get; }
    public string Name { get; }
    public bool IsStatic { get; }
    
    /// <summary>
    /// The signature of the method. Stored here so it doesn't need to be computed more than once.
    /// </summary>
    public string Signature { get; set; }

    public MethodDeclarationSymbol(TypeReferenceSymbol? type, GetAccessibility accessibility,
        string name, bool isStatic)
    {
        Type = type;
        Accessibility = accessibility;
        Name = name;
        IsStatic = isStatic;
        Signature = "";
    }
}