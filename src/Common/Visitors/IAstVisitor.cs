using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Visitors;

/// <summary>
/// A visitor that supports all AST nodes.
/// </summary>
public interface IAstVisitor<T>
{
    public T Visit(Node node) => node.AcceptVisitor(this);

    /// <summary>
    /// Visits a member access.
    /// </summary>
    public T MemberAccess(MemberAccessNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits a class declaration.
    /// </summary>
    public T ClassDefinition(ClassTypeDefinitionNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits an externed type declaration.
    /// </summary>
    public T ExternedTypeDefinition(ExternedTypeDefinitionNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits a struct declaration.
    /// </summary>
    public T StructDefinition(StructTypeDefinitionNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits a type reference.
    /// </summary>
    public T TypeReference(TypeReferenceNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits type property members.
    /// </summary>
    public T Property(PropertyNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a method declaration.
    /// </summary>
    public T MethodDeclaration(MethodDeclarationNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits an externed method mapping.
    /// </summary>
    public T ExternedMethodMapping(ExternedMethodNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits a method declaration.
    /// </summary>
    public T ConstructorDeclaration(ConstructorDeclarationNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a local variable.
    /// </summary>
    public T LocalVariable(LocalVariableDeclarationNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a method call.
    /// </summary>
    public T MethodCall(MethodCallNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a variable assignment.
    /// </summary>
    public T VariableAssignment(VariableAssignmentNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits an integer.
    /// </summary>
    public T Integer(IntegerNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a new class instance initializer.
    /// </summary>
    public T NewClassInstance(NewClassInitializerNode node) => throw new NotImplementedException();
}