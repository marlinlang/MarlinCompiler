using System.Data;
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
        /// This is the scope of the type.
        /// </summary>
        public Scope? Scope { get; set; }

        public SemType? GenericTypeParameter { get; set; } = GenericTypeParameter;

        public override string ToString()
        {
            return GenericTypeParameter != null 
                ? $"{Name}<{GenericTypeParameter}>" 
                : Name;
        }
    }

    /// <summary>
    /// Represents a symbol.
    /// </summary>
    public record Symbol(SymbolKind Kind, SemType Type, string Name, Scope Scope, Node Node)
    {
        public static Symbol UnknownType { get; } =
            new(SymbolKind.ClassType, new SemType("???", null), "???", new Scope(null), null!);

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

        public List<(string, string?)> Generics { get; } = new();

        /// <summary>
        /// Searches for a symbol.
        /// </summary>
        public Symbol? LookupSymbol(string name, bool thisScopeOnly)
        {
            Symbol? found = Symbols.Find(x => x.Name == name);

            if (found != null)
            {
                return found;
            }

            return thisScopeOnly ? null : Parent?.LookupSymbol(name, false);
        }

        /// <summary>
        /// Searches for a type.
        /// </summary>
        public Symbol LookupType(SemType type)
        {
            (string, string?) generic = Generics.Find(x => x.Item1 == type.Name);
            if (generic != default)
            {
                if (generic.Item2 != null)
                {
                    return LookupType(new SemType(generic.Item2, null));
                }
                
                return new Symbol(SymbolKind.GenericTypeParam, type, generic.Item1, this, null!);
            }

            Symbol? found = Symbols.Find(
                x => x.Name == type.Name
                     && x.Kind is SymbolKind.ClassType or SymbolKind.StructType or SymbolKind.ExternType
            );

            if (found != null)
            {
                return found;
            }

            return Parent?.LookupType(type) ?? Symbol.UnknownType;
        }

        /// <summary>
        /// Adds a symbol to this scope. Also used for adding types.
        /// </summary>
        public void AddSymbol(Symbol symbol) => Symbols.Add(symbol);

        /// <summary>
        /// Adds a new generic param.
        /// </summary>
        public void AddGenericParam(string name) => Generics.Add((name, null));

        public Scope CloneScope()
        {
            Scope other = new(Parent)
            {
                // Name
                DebugName = $"Clone of {DebugName}"
            };

            // Symbols
            foreach (Symbol sym in Symbols)
            {
                Symbol newSym = sym with {};

                if (newSym.Scope == this)
                {
                    newSym.Scope = other;
                }
                
                other.AddSymbol(newSym);
            }
            
            // Generics
            foreach ((string, string?) generic in Generics)
            {
                other.AddGenericParam(generic.Item1);
                other.Generics[^1] = (generic.Item1, generic.Item2);
            }

            return other;
        }
    }

    private record SymbolMetadata(Symbol Symbol) : INodeMetadata;
}