using MarlinCompiler.Common.FileLocations;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A node that represents a compilation unit.
/// </summary>
public class CompilationUnitNode : ContainerNode
{
    public CompilationUnitNode(string fullName, (string, FileLocation)[] dependencies)
    {
        FullName     = fullName;
        Dependencies = dependencies;
    }

    /// <summary>
    /// The full name of the module, including parent modules, separated by double colons.
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// The names of other needed compilation units.
    /// string is the name of the dependency, FileLocation is the location where the dependency was requested
    /// </summary>
    public (string, FileLocation)[] Dependencies { get; }
}