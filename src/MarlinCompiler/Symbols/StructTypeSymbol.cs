using JetBrains.Annotations;

namespace MarlinCompiler.Symbols;

public class StructTypeSymbol : TypeSymbol
{
    public StructTypeSymbol(string name, MemberVisibility visibility) : base(name, visibility)
    {
    }
}