using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Visitors;

/// <summary>
/// This is a helper class visitor which lets you override only the methods you need.
/// The name comes from the fact that this class never throws <see cref="NotImplementedException"/>,
/// as opposed to the base class which does.
/// </summary>
public class NoNieVisitor : AstVisitor<None>
{
    public override None MemberAccess(MemberAccessNode node)
    {
        return None.Null;
    }

    public override None ClassDefinition(ClassTypeDefinitionNode node)
    {
        return None.Null;
    }

    public override None ExternTypeDefinition(ExternTypeDefinitionNode node)
    {
        return None.Null;
    }

    public override None StructDefinition(StructTypeDefinitionNode node)
    {
        return None.Null;
    }

    public override None TypeReference(TypeReferenceNode node)
    {
        return None.Null;
    }

    public override None Property(PropertyNode node)
    {
        return None.Null;
    }

    public override None MethodDeclaration(MethodDeclarationNode node)
    {
        return None.Null;
    }

    public override None ExternMethodMapping(ExternMethodNode node)
    {
        return None.Null;
    }

    public override None ConstructorDeclaration(ConstructorDeclarationNode node)
    {
        return None.Null;
    }

    public override None LocalVariable(LocalVariableDeclarationNode node)
    {
        return None.Null;
    }

    public override None MethodCall(MethodCallNode node)
    {
        return None.Null;
    }

    public override None VariableAssignment(VariableAssignmentNode node)
    {
        return None.Null;
    }

    public override None Integer(IntegerNode node)
    {
        return None.Null;
    }

    public override None NewClassInstance(NewClassInitializerNode node)
    {
        return None.Null;
    }

    public override None ReturnStatement(ReturnStatementNode node)
    {
        return None.Null;
    }
}