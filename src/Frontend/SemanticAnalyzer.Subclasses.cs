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

        public SemType? GenericTypeParameter { get; private set; } = GenericTypeParameter;

        public override string ToString()
        {
            return GenericTypeParameter != null 
                ? $"{AttemptResolveGeneric().Name}<{GenericTypeParameter}>" 
                : AttemptResolveGeneric().Name;
        }

        public SemType AttemptResolveGeneric()
        {
            if (GenericTypeParameter != null)
            {
                if (GenericTypeParameter.Scope == null)
                {
                    GenericTypeParameter.Scope = Scope;
                }

                GenericTypeParameter = GenericTypeParameter.AttemptResolveGeneric();
            }

            SemType found = Scope?.LookupType(this)?.Type ?? this;
            return found.Name != Name ? found : this;
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

        private List<(string, SemType?)> _generics = new();

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
        public Symbol? LookupType(SemType type)
        {
            (string, SemType?) generic = _generics.Find(x => x.Item1 == type.Name);
            if (generic != default)
            {
                return generic.Item2 != null
                    ? LookupType(generic.Item2)
                    : new Symbol(SymbolKind.GenericTypeParam, type, "T", this, null!);
            }

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
        /// Adds a new generic param.
        /// </summary>
        public void AddGenericParam(string name) => _generics.Add((name, null));

        /// <summary>
        /// Sets the argument of the given generic param.
        /// </summary>
        public void SetGenericArg(string name, SemType value)
        {
            for (int i = 0; i < _generics.Count; i++)
            {
                if (_generics[i].Item1 != name) continue;
                
                _generics[i] = (name, value);
                return;
            }

            throw new InvalidOperationException("Generic param does not exist");
        }

        /// <summary>
        /// Sets the argument of the given generic param.
        /// </summary>
        public void SetGenericArg(int idx, SemType value)
        {
            if (idx >= _generics.Count)
            {
                throw new InvalidOperationException("Generic param does not exist");
            }

            SetGenericArg(_generics[idx].Item1, value);
        }

        public Scope CloneScope()
        {
            Scope other = new(Parent);

            // Name
            other.DebugName = $"Clone of {DebugName}";

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
            foreach ((string, SemType?) generic in _generics)
            {
                other.AddGenericParam(generic.Item1);

                if (generic.Item2 != null)
                {
                    other.SetGenericArg(generic.Item1, generic.Item2 with {});
                }
            }

            return other;
        }
    }

    private record SymbolMetadata(Symbol Symbol) : INodeMetadata;
}