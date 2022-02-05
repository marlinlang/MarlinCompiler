namespace MarlinCompiler.Common.Semantics.Symbols;

public class TypeReferenceSymbol : Symbol
{
    public TypeDeclarationSymbol? Type { get; }
    public TypeDeclarationSymbol? GenericType { get; }
    public bool IsArray { get; }

    public TypeReferenceSymbol(TypeDeclarationSymbol? type, TypeDeclarationSymbol? genericType, bool isArray)
    {
        Type = type;
        GenericType = genericType;
        IsArray = isArray;
    }
}