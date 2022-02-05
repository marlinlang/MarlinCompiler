namespace MarlinCompiler.Common.Semantics.Symbols;

public class TypeDeclarationSymbol : Symbol
{
    public string Name { get; }

    protected TypeDeclarationSymbol(string name)
    {
        Name = name;
    }
}