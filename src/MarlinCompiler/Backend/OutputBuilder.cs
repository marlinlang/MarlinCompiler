using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Backend;

/// <summary>
/// This class manages the LLVM IR generation for Marlin programs.
/// </summary>
public sealed partial class OutputBuilder
{
    public OutputBuilder(Node root, string path)
    {
        _root = root;
        _path = path;
        MessageCollection = new MessageCollection();
    }

    /// <summary>
    /// The root node of the program.
    /// </summary>
    private readonly Node _root;

    /// <summary>
    /// The project path.
    /// </summary>
    private readonly string _path;

    /// <summary>
    /// LLVM compilation messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    public void BuildLlvm()
    {
        
    }
}