using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

/// <summary>
/// Marlin semantic analyzer.
/// </summary>
public class Analyzer : AstVisitor<None>
{
    public Analyzer(IEnumerable<CompilationUnitNode> compilationUnits)
    {
        _compilationUnits = compilationUnits;
    }
    
    /// <summary>
    /// The compilation units.
    /// </summary>
    private readonly IEnumerable<CompilationUnitNode> _compilationUnits;

    public void Analyze()
    {
        // TODO: Analyze the compilation units.
        
        foreach (CompilationUnitNode compilationUnit in _compilationUnits)
        {
            Visit(compilationUnit);
        }
    }
    
    public override None MemberAccess(MemberAccessNode node)
    {
        throw new NotImplementedException();
    }

    public override None ClassDefinition(ClassTypeDefinitionNode node)
    {
        throw new NotImplementedException();
    }

    public override None ExternTypeDefinition(ExternTypeDefinitionNode node)
    {
        throw new NotImplementedException();
    }

    public override None StructDefinition(StructTypeDefinitionNode node)
    {
        throw new NotImplementedException();
    }

    public override None TypeReference(TypeReferenceNode node)
    {
        throw new NotImplementedException();
    }

    public override None Property(PropertyNode node)
    {
        throw new NotImplementedException();
    }

    public override None MethodDeclaration(MethodDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public override None ExternMethodMapping(ExternMethodNode node)
    {
        throw new NotImplementedException();
    }

    public override None ConstructorDeclaration(ConstructorDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public override None LocalVariable(LocalVariableDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public override None MethodCall(MethodCallNode node)
    {
        throw new NotImplementedException();
    }

    public override None VariableAssignment(VariableAssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public override None Integer(IntegerNode node)
    {
        throw new NotImplementedException();
    }

    public override None NewClassInstance(NewClassInitializerNode node)
    {
        throw new NotImplementedException();
    }
}