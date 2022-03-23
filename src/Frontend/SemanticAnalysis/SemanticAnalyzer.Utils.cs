using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

public sealed partial class SemanticAnalyzer
{
    /// <summary>
    /// The internal stack of scopes.
    /// </summary>
    private readonly Stack<Scope> _scopes = new();

    /// <summary>
    /// The current scope.
    /// </summary>
    private Scope CurrentScope => _scopes.Peek();

    /// <summary>
    /// The current compilation unit.
    /// </summary>
    private CompilationUnitNode _currentCompilationUnit = null!;

    /// <summary>
    /// A list of the necessary modules.
    /// </summary>
    private List<(string, FileLocation)> _usedDependencies = new();

    /// <summary>
    /// This is a hack: to make sure we don't report unused warnings about dependencies that don't exist, we'll
    /// plop them in this list.
    /// </summary>
    private List<string> _dependenciesThatWereNotFound = new();
    
    /// <summary>
    /// Adds a new scope on top.
    /// </summary>
    private Scope PushScope(string name)
    {
        Scope scope = new(name, _scopes.Count == 0 ? null : _scopes.Peek());
        _scopes.Push(scope);
        return scope;
    }

    /// <summary>
    /// Adds the given scope to the scope stack without changing the scope itself.
    /// </summary>
    private void UseScope(Scope scope) => _scopes.Push(scope);
    
    /// <summary>
    /// Removes the topmost scope.
    /// </summary>
    private void PopScope() => _scopes.Pop();
    
    /// <summary>
    /// Adds the symbol in the metadata to the topmost scope. 
    /// </summary>
    private void AddSymbolToScope(SymbolMetadata metadata)
    {
        if (metadata.Symbol == null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }

        _scopes.Peek().AddSymbol(metadata.Symbol);
    }

    /// <summary>
    /// Checks if the dependencies in the given compilation unit exist.
    /// </summary>
    private void CheckDependencies(CompilationUnitNode compilationUnit)
    {
        string[] allCompilationUnits = new string[_root.Children.Count];
        for (int i = 0; i < allCompilationUnits.Length; i++)
        {
            allCompilationUnits[i] = ((CompilationUnitNode) _root.Children[i]).FullName;
        }
        
        foreach ((string dependencyName, FileLocation location) in compilationUnit.Dependencies)
        {
            if (allCompilationUnits.Contains(dependencyName)) continue;
            
            _dependenciesThatWereNotFound.Add(dependencyName);
            MessageCollection.Error($"Unknown module {dependencyName}", location);
        }
    }

    /// <summary>
    /// This method checks if the given type's module is in the dependency list.
    /// </summary>
    private void CheckCanUseType(string name, FileLocation errorLocation)
    {
        // this works, LastIndexOf returns -1 if it isn't found
        // so it'll start from 0 if it can't find it :)
        int lioColon = name.LastIndexOf(':') + 1;
        string originalName = name;
        name = name[lioColon..];

        if (originalName == $"{_currentCompilationUnit.FullName}::{name}")
        {
            return;
        }

        var dep = Array.Find(_currentCompilationUnit.Dependencies, x => originalName == $"{x.Item1}::{name}");
        if (dep != default)
        {
            _usedDependencies.Add(dep);
            return;
        }

        if (lioColon == -1)
        {
            MessageCollection.Error($"Unknown type {originalName} - did you forget to import a module?", errorLocation);
        }
        else
        {
            string moduleName = originalName[..(lioColon - 2)];
            MessageCollection.Error($"Unknown type {originalName} - did you forget to import a module ({moduleName})?", errorLocation);
        }
    }

    /// <summary>
    /// Reports warnings about unused dependencies.
    /// </summary>
    private void CheckUnusedDependencies(CompilationUnitNode unit)
    {
        foreach ((string name, FileLocation importLocation) in unit.Dependencies)
        {
            (string, FileLocation) found = _usedDependencies.Find(x => x.Item1 == name);
            if (found == default && !_dependenciesThatWereNotFound.Contains(name))
            {
                MessageCollection.Warn($"Unused dependency {name}", importLocation);
            }
        }
    }
    
    /// <summary>
    /// Checks whether two types are compatible. Supports generics.
    /// </summary>
    private static (bool compatible, string expectedFullName, string givenFullName) AreTypesCompatible(SemType expected, SemType given)
    {
        if (expected.Name != given.Name)
        {
            // Obvious: name mismatch!
            // TODO: Inheritance
            return (false, expected.ToString(), given.ToString());
        }

        if (expected.GenericTypeParameter is null != given.GenericTypeParameter is null)
        {
            // One type has generic param, but the other doesn't
            return (false, expected.ToString(), given.ToString());
        }
        
        if (expected.GenericTypeParameter == null)
        {
            // No generic param/arg in either
            return (true, expected.ToString(), given.ToString());
        }
        
        // Both have generic args, make sure they're compatible
        (bool compatible, string _, string _) = AreTypesCompatible(
            expected.GenericTypeParameter!,
            given.GenericTypeParameter!
        );
        return (compatible, expected.ToString(), given.ToString());
    }

    /// <summary>
    /// Recursive method that replaces all instances of typeName with the provided type.
    /// </summary>
    /// <param name="root">The scope to replace in.</param>
    /// <param name="typeName">The type name to replace.</param>
    /// <param name="with">The type to replace it with.</param>
    private static void ReplaceAllOccurrencesOfType(Scope root, string typeName, SemType with)
    {
        foreach (Symbol sym in root.Symbols)
        {
            if (sym.Type.Name == typeName)
            {
                sym.Type = with;
            }

            if (sym.Type.GenericTypeParameter?.Name == typeName)
            {
                sym.Type.GenericTypeParameter = with;
            }

            // Some symbols carry the same scope as their parent
            // E.g. properties carry the symbol of the type they're in
            if (sym.Scope != root)
            {
                ReplaceAllOccurrencesOfType(sym.Scope, typeName, with);
            }
        }
    }
    
    /// <summary>
    /// Helper method.
    /// </summary>
    private static SemType GetSemType(TypeReferenceNode node)
    {
        if (node is VoidTypeReferenceNode)
        {
            return SpecialTypes.Void.Type;
        }
        
        return new SemType(
            node.FullName,
            node.GenericTypeName != null
                ? GetSemType(node.GenericTypeName)
                : null
            );
    }
}