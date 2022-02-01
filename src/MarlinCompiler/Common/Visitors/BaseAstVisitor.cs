using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Visitors;

public class BaseAstVisitor<T> : IAstVisitor<T>
{
    public virtual T Visit(Node node) => node.AcceptVisitor(this);

    public virtual T MemberAccess(MemberAccessNode node)
    {
        throw new NotImplementedException();
    }

    public virtual T ClassDefinition(ClassTypeDefinitionNode node)
    {
        throw new NotImplementedException();
    }

    public virtual T StructDefinition(StructTypeDefinitionNode node)
    {
        throw new NotImplementedException();
    }

    public virtual T TypeReference(TypeReferenceNode node)
    {
        throw new NotImplementedException();
    }

    public virtual T Property(PropertyNode node)
    {
        throw new NotImplementedException();
    }

    public virtual T MethodDeclaration(MethodDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public virtual T LocalVariable(LocalVariableDeclaration node)
    {
        throw new NotImplementedException();
    }

    public virtual T MethodCall(MethodCallNode node)
    {
        throw new NotImplementedException();
    }

    public virtual T VariableAssignment(VariableAssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public virtual T Tuple(TupleNode node)
    {
        throw new NotImplementedException();
    }

    public virtual T Integer(IntegerNode node)
    {
        throw new NotImplementedException();
    }
}