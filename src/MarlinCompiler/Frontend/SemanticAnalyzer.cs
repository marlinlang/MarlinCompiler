using System.Data;
using System.Net.Http.Headers;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend;

public sealed partial class SemanticAnalyzer : IAstVisitor<Node>
{
    public SemanticAnalyzer()
    {
        MessageCollection = new MessageCollection();
    }

    /// <summary>
    /// A pass for the analyzer.
    /// </summary>
    private enum AnalyzerPass
    {
        DefineTypes,
        DefineTypeMembers,
        VisitTypeMembers
    }

    /// <summary>
    /// A collection of the analyzer messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    /// <summary>
    /// The current analyzer pass.
    /// </summary>
    private AnalyzerPass _pass;
    
    public void Analyze(Node root)
    {
        foreach (AnalyzerPass pass in Enum.GetValues<AnalyzerPass>())
        {
            _pass = pass;
            Visit(root);
        }
    }
    
    public Node Visit(Node root)
    {
        return root.AcceptVisitor(this);
    }
}