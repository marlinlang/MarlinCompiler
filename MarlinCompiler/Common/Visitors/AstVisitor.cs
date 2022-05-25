using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Visitors;

/// <summary>
/// A visitor that supports all AST nodes.
/// </summary>
public abstract class AstVisitor<T>
{
    public T Visit(Node node)
    {
        return node.AcceptVisitor(this);
    }

    /// <summary>
    /// Visits a member access.
    /// </summary>
    public abstract T MemberAccess(MemberAccessNode node);

    /// <summary>
    /// Visits a class declaration.
    /// </summary>
    public abstract T ClassDefinition(ClassTypeDefinitionNode node);

    /// <summary>
    /// Visits an extern type declaration.
    /// </summary>
    public abstract T ExternTypeDefinition(ExternTypeDefinitionNode node);

    /// <summary>
    /// Visits a struct declaration.
    /// </summary>
    public abstract T StructDefinition(StructTypeDefinitionNode node);

    /// <summary>
    /// Visits a type reference.
    /// </summary>
    public abstract T TypeReference(TypeReferenceNode node);

    /// <summary>
    /// Visits type property members.
    /// </summary>
    public abstract T Property(PropertyNode node);

    /// <summary>
    /// Visits a method declaration.
    /// </summary>
    public abstract T MethodDeclaration(MethodDeclarationNode node);

    /// <summary>
    /// Visits an extern method mapping.
    /// </summary>
    public abstract T ExternMethodMapping(ExternMethodNode node);

    /// <summary>
    /// Visits a constructor declaration.
    /// </summary>
    public abstract T ConstructorDeclaration(ConstructorDeclarationNode node);

    /// <summary>
    /// Visits a local variable.
    /// </summary>
    public abstract T LocalVariable(LocalVariableDeclarationNode node);

    /// <summary>
    /// Visits a method call.
    /// </summary>
    public abstract T MethodCall(MethodCallNode node);

    /// <summary>
    /// Visits a variable assignment.
    /// </summary>
    public abstract T VariableAssignment(VariableAssignmentNode node);

    /// <summary>
    /// Visits an integer.
    /// </summary>
    public abstract T Integer(IntegerNode node);

    /// <summary>
    /// Visits a new class instance initializer.
    /// </summary>
    public abstract T NewClassInstance(NewClassInitializerNode node);

    /// <summary>
    /// Visits a return statement.
    /// </summary>
    public abstract T ReturnStatement(ReturnStatementNode node);
}