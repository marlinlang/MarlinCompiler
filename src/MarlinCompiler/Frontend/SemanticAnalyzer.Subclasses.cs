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
        ExternMethod,
        Constructor,
        Property,
        Variable,
        GenericTypeParam,
        Instance
    }

    /// <summary>
    /// Represents a semantic type.
    /// </summary>
    public record SemType(string Name, SemType? GenericTypeParameter)
    {
        /// <summary>
        /// This might contain a value - always check!
        /// </summary>
        public Scope? Scope { get; set; }

        public virtual bool Equals(SemType? other)
        {
            if (other is null) return false;

            return other.Name == Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Represents a symbol.
    /// </summary>
    public record Symbol(SymbolKind Kind, SemType Type, string Name, Scope Scope, Node Node)
    {
        public static Symbol UnknownType { get; } = new(SymbolKind.ClassType, new SemType("???", null), "???", new Scope(null), null!);
        
        public SemType Type { get; set; } = Type;
        public Scope Scope { get; set; } = Scope;
    }

    public class Scope
    {
        public Scope(Scope? parent)
        {
            Parent = parent;
        }

        public string DebugName { get; set; } = "Unnamed";
        
        public Scope? Parent { get; set; }
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

        public Symbol? LookupType(SemType type)
        {
            Symbol? found = Symbols.Find(
                x => x.Name == type.Name
                     && x.Kind is SymbolKind.ClassType or SymbolKind.StructType or SymbolKind.ExternType
            );

            if (found != null)
            {
                return found;
            }
            
            return Parent?.LookupType(type);
        }
        
        /// <summary>
        /// Adds a symbol to this scope. Also used for adding types.
        /// </summary>
        public void AddSymbol(Symbol symbol) => Symbols.Add(symbol);

        /// <summary>
        /// Clones the scope.
        /// </summary>
        public Scope CloneScope()
        {
            Scope newScope = new(Parent);

            // Clone symbols
            foreach (Symbol sym in Symbols)
            {
                Symbol newSym = sym with {};

                if (newSym.Scope == this)
                {
                    newSym.Scope = newScope;
                }
                else
                {
                    newSym.Scope = newSym.Scope.CloneScope();
                    newSym.Scope.Parent = newScope;
                }

                if (newSym.Type.Scope == this)
                {
                    newSym.Type = newSym.Type with {Scope = newScope};
                }
                
                newScope.Symbols.Add(newSym);
            }
            
            return newScope;
        }
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