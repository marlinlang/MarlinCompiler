using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Frontend;

public sealed partial class SemanticAnalyzer
{
    public SemanticAnalyzer(Node root)
    {
        _root = root;
        MessageCollection = new MessageCollection();
    }

    /// <summary>
    /// A collection of the analyzer messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }
    
    /// <summary>
    /// The file root.
    /// </summary>
    private readonly Node _root;

    /// <summary>
    /// The current analyzer pass.
    /// </summary>
    private AnalyzerPass _pass;

    /// <summary>
    /// Starts analyzing the program.
    /// </summary>
    public void Analyze()
    {
        PushScope("<MARLIN_PROGRAM>");
        foreach (AnalyzerPass pass in Enum.GetValues<AnalyzerPass>())
        {
            _pass = pass;
            Visit(_root);
        }
        PopScope();
    }
}