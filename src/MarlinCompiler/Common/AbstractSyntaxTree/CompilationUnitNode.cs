using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A node that represents a compilation unit.
/// </summary>
public class CompilationUnitNode : ContainerNode
{
    /// <summary>
    /// The full name of the module, including parent modules, separated by double colons.
    /// </summary>
    public string FullName { get; }
    
    /// <summary>
    /// The names of other needed compilation units.
    /// </summary>
    public string[] Dependencies { get; }

    public CompilationUnitNode(string fullName, string[] dependencies)
    {
        FullName = fullName;
        Dependencies = dependencies;
    }
}