using System.Data;
using System.Net.Http.Headers;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Intermediate;

public sealed class SemanticAnalyzer : IAstVisitor<Node>
{
    public MessageCollection MessageCollection { get; }

    public SemanticAnalyzer()
    {
        MessageCollection = new MessageCollection();
    }

    public void Visit(Node root)
    {
        return;
    }
}