using MarlinCompiler.Common;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

/// <summary>
/// Analyzer pass.
/// </summary>
public interface IPass
{
    public ScopeManager ScopeManager { get; }
    public AstVisitor<None> Visitor { get; }
}