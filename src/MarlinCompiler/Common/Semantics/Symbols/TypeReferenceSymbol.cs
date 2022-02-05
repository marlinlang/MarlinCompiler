namespace MarlinCompiler.Common.Semantics.Symbols;

public class TypeReferenceSymbol : Symbol
{
    public TypeDeclarationSymbol? Type { get; }

    public TypeReferenceSymbol(TypeDeclarationSymbol? type)
    {
        Type = type;
    }
}