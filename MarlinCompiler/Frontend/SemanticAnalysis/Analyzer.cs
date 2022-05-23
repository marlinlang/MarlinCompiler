using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Messages;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

/// <summary>
/// Marlin semantic analyzer.
/// </summary>
public sealed partial class Analyzer
{
    public Analyzer(IEnumerable<CompilationUnitNode> compilationUnits)
    {
        _compilationUnits = compilationUnits;
        MessageCollection = new MessageCollection();

        CurrentVisitor = null!;
    }

    /// <summary>
    /// Analysis messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    /// <summary>
    /// The visitor that is currently being used to visit the AST.
    /// </summary>
    public AstVisitor<None> CurrentVisitor { get; private set; }

    /// <summary>
    /// The compilation units.
    /// </summary>
    private readonly IEnumerable<CompilationUnitNode> _compilationUnits;

    public void Analyze()
    {
        /*
         * Here's how it works:
         * 1. Declaration pass - when parsed, methods & properties can't know their own return types yet.
         *    To deal with that, we will go and find these return types here.
         * 2. Everything else pass - we can now resolve all types and methods. We can do type and
         *    semantic checking.
         */

        DeclarationsPass declarationsPass = new(this);
        MainPass mainPass = new(this);

        foreach (CompilationUnitNode compilationUnit in _compilationUnits)
        {
            UseVisitor(declarationsPass, compilationUnit);
        }
        foreach (CompilationUnitNode compilationUnit in _compilationUnits)
        {
            UseVisitor(mainPass, compilationUnit);
        }
    }

    private void UseVisitor(AstVisitor<None> visitor, Node node)
    {
        CurrentVisitor = visitor;
        CurrentVisitor.Visit(node);
    }
}