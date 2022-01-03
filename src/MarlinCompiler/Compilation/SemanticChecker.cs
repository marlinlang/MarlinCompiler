using MarlinCompiler.Ast;
using MarlinCompiler.Compilation;

namespace MarlinCompiler.Compilation;

internal class SemanticChecker : BaseAstVisitor<AstNode>
{
    public CompileMessages Messages { get; }
    private readonly Builder _builder;
    
    public SemanticChecker(Builder builder)
    {
        _builder = builder;
    }
    
    public override AstNode VisitClassDeclarationNode(ClassDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public override AstNode VisitMemberAccessNode(MemberAccessNode node)
    {
        throw new NotImplementedException();
    }

    public override AstNode VisitMethodDeclarationNode(MethodDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public override AstNode VisitMethodPrototypeNode(MethodPrototypeNode node)
    {
        throw new NotImplementedException();
    }

    public override AstNode VisitMethodCallNode(MethodCallNode node)
    {
        throw new NotImplementedException();
    }

    public override AstNode VisitVariableAssignmentNode(VariableAssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public override AstNode VisitVariableDeclarationNode(VariableDeclarationNode node)
    {
        throw new NotImplementedException();
    }
}