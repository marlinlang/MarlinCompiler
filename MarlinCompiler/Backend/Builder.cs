using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Messages;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Backend;

public sealed class Builder
{
    public Builder(IEnumerable<CompilationUnitNode> compilationUnits)
    {
        _compilationUnits = compilationUnits;
        MessageCollection = new MessageCollection();
        Tools             = new BuilderTools();
        CurrentPass       = null!;
    }

    /// <summary>
    /// Analysis messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    /// <summary>
    /// The visitor that is currently being used to visit the AST.
    /// </summary>
    public AstVisitor<None> CurrentPass { get; private set; }

    /// <summary>
    /// The builder tools.
    /// </summary>
    public BuilderTools Tools { get; }

    /// <summary>
    /// The compilation units.
    /// </summary>
    private readonly IEnumerable<CompilationUnitNode> _compilationUnits;

    public void Build()
    {
        TypeDeclarationPass declarationsPass = new(Tools);
        MainPass mainPass = new(Tools);

        foreach (CompilationUnitNode compilationUnit in _compilationUnits)
        {
            InvokePass(declarationsPass, compilationUnit);
        }
        foreach (CompilationUnitNode compilationUnit in _compilationUnits)
        {
            InvokePass(mainPass, compilationUnit);
        }
    }

    private void InvokePass(AstVisitor<None> pass, Node node)
    {
        CurrentPass = pass;
        pass.Visit(node);
    }
}