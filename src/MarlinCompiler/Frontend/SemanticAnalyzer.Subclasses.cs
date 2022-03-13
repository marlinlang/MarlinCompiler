using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Frontend;

public sealed partial class SemanticAnalyzer
{
    private enum AnalyzerPass
    {
        DefineTypes,
        DefineTypeMembers,
        EnterTypeMembers
    }

    public enum SymbolKind
    {
        ClassType,
        StructType,
        ExternType,
        Method,
        Constructor,
        Property,
        Variable
    }

    /// <summary>
    /// Represents a semantic type.
    /// </summary>
    public record SemType(string Name, string? GenericTypeParam);
    
    /// <summary>
    /// Represents a symbol.
    /// </summary>
    public record Symbol(SymbolKind Kind, SemType Type, string Name, Scope? Scope, Node Node);

    public class Scope
    {
        public Scope(Scope? parent)
        {
            Parent = parent;
        }
        
        public Scope? Parent { get; }
        public List<Symbol> Symbols { get; } = new();

        public Symbol? LookupSymbol(string name, bool thisScopeOnly)
        {
            Symbol? found = Symbols.Find(x => x.Name == name);

            if (found != null)
            {
                return found;
            }
            
            return thisScopeOnly ? null : Parent?.LookupSymbol(name, false);
        }

        public void AddSymbol(Symbol symbol) => Symbols.Add(symbol);
    }
    
    private class SymbolMetadata : NodeMetadata
    {
        public SymbolMetadata(Symbol symbol)
        {
            Symbol = symbol;
        }

        public Symbol Symbol { get; set; }
    }
}