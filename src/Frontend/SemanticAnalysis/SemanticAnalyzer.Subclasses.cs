using JetBrains.Annotations;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

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
        public string Name { get; } = Name;
        public SemType? GenericTypeParameter { get; set; } = GenericTypeParameter;

        /// <summary>
        /// True if this is a generic param type (e.g. T)
        /// </summary>
        public bool IsGenericParam { get; set; } = false;
        
        /// <summary>
        /// This is the scope of the type.
        /// </summary>
        public Scope? Scope { get; set; }

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
            new(SymbolKind.ClassType, new SemType("???", null), "???", new Scope("???", null), null!);

        public SemType Type { get; set; } = Type;
        public Scope Scope { get; set; } = Scope;
    }

    public class Scope
    {
        public Scope(string name, Scope? parent)
        {
            Name = name;
            Parent = parent;
        }

        public string Name { get; }

        public Scope? Parent { get; set; }
        public List<Symbol> Symbols { get; } = new();

        public List<string> Generics { get; } = new();

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
            string? generic = Generics.Find(x => x == type.Name);
            if (generic != default)
            {
                return new Symbol(SymbolKind.GenericTypeParam, type, generic, this, null!);
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
        public void AddGenericParam(string name) => Generics.Add(name);

        public Scope CloneScope()
        {
            Scope other = new(Name, Parent);

            // Symbols
            foreach (Symbol sym in Symbols)
            {
                Symbol newSym = sym with {};

                if (newSym.Scope == this)
                {
                    newSym.Scope = other;
                }

                // Constructors have null types!!!!
                // ReSharper disable once ConstantConditionalAccessQualifier
                if (newSym.Type?.Scope == this)
                {
                    newSym.Type.Scope = other;
                }
                
                other.AddSymbol(newSym);
            }
            
            // Generics
            foreach (string param in Generics)
            {
                other.AddGenericParam(param);
            }

            return other;
        }
    }

    private record SymbolMetadata([UsedImplicitly] Symbol Symbol) : INodeMetadata;
}