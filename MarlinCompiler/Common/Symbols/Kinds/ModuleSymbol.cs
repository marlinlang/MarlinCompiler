using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents a module.
/// </summary>
public sealed class ModuleSymbol : NamedSymbol
{
    public ModuleSymbol(CompilationUnitNode node) : base(node.FullName)
    {
        Dependencies = new ModuleSymbol[node.Dependencies.Length];
    }

    /// <summary>
    /// The dependencies of the module.
    /// </summary>
    /// <remarks>By default, this is an empty array with the size of the dependency array
    /// of the module node that was used to create this symbol.</remarks>
    public ModuleSymbol[] Dependencies { get; }
}