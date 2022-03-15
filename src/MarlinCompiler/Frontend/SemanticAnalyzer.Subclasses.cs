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
    public record SemType(string Name, SemType? GenericTypeParam)
    {
        /// <summary>
        /// This might contain a value - always check!
        /// </summary>
        public Scope? Scope { get; set; }
        
        public virtual bool EqualsXXX(SemType? other)
        {
            if (other is null) return false;

            return other.Name == Name;
        }

        public override string ToString()
        {
            if (GenericTypeParam != null)
            {
                return $"{Name}<{GenericTypeParam}>";
            }
            else
            {
                return Name;
            }
        }
    }

    /// <summary>
    /// Represents a symbol.
    /// </summary>
    public record Symbol(SymbolKind Kind, SemType Type, string Name, Scope Scope, Node Node)
    {
        public Scope Scope { get; set; } = Scope;
    }

    public class Scope
    {
        public Scope(Scope? parent)
        {
            Parent = parent;
        }
        
        public Scope? Parent { get; }
        public List<Symbol> Symbols { get; } = new();

        /// <summary>
        /// Dictionary of generic parameters. The key is the type alias (e.g. T). The value is, when the scope is
        /// cloned for a variable instance, the type used in place of T.
        /// This is a list because it needs to be looked up by index.
        /// </summary>
        private List<(string, SemType?)> _genericParams = new();
        
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
            // Generics

            // C# goes out of its way to make a STACK OVERFLOW HERE
            string typeName = type.Name;

            (string, SemType?) foundGenericParam = _genericParams.Find(x => typeName == x.Item1);
            if (foundGenericParam.Item1 != null)
            {
                if (foundGenericParam.Item2 == null)
                {
                    return new Symbol(SymbolKind.GenericTypeParam, type, "$", this, null!);
                }
                else if (type != foundGenericParam.Item2)
                {
                    return LookupType(foundGenericParam.Item2);
                }
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
        /// Registers a new generic param.
        /// </summary>
        public void AddGenericParam(string name)
        {
            _genericParams.Add((name, null));
        }

        /// <summary>
        /// Assigns the alias to the given generic param.
        /// </summary>
        public void SetGenericArgument(string param, SemType arg)
        {
            int idx = _genericParams.FindIndex(x => x.Item1 == param);

            if (idx == -1)
            {
                throw new InvalidOperationException($"Unknown param {param}");
            }
            
            _genericParams[idx] = (param, arg);
        }

        /// <summary>
        /// Assigns the alias to the generic param at the given index.
        /// </summary>
        public void SetGenericArgument(int idx, SemType arg)
        {
            string param = _genericParams[idx].Item1;
            _genericParams[idx] = (param, arg);
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
            return (Scope)MemberwiseClone();
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