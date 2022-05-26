using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Backend;

public class MainPass : AstVisitor<None>
{
    public MainPass(BuilderTools tools)
    {
        _tools = tools;
    }

    private readonly BuilderTools _tools;
    
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

    public override None ReturnStatement(ReturnStatementNode node)
    {
        throw new NotImplementedException();
    }
}