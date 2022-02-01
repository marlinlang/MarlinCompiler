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
    public virtual T MemberAccess(MemberAccessNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits a class declaration.
    /// </summary>
    public virtual T ClassDefinition(ClassTypeDefinitionNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits a struct declaration.
    /// </summary>
    public virtual T StructDefinition(StructTypeDefinitionNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits a type reference.
    /// </summary>
    public virtual T TypeReference(TypeReferenceNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits type property members.
    /// </summary>
    public virtual T Property(PropertyNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a method declaration.
    /// </summary>
    public virtual T MethodDeclaration(MethodDeclarationNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a local variable.
    /// </summary>
    public virtual T LocalVariable(LocalVariableDeclarationNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a method call.
    /// </summary>
    public virtual T MethodCall(MethodCallNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a variable assignment.
    /// </summary>
    public virtual T VariableAssignment(VariableAssignmentNode node) => throw new NotImplementedException();

    /// <summary>
    /// Visits a tuple.
    /// </summary>
    public virtual T Tuple(TupleNode node) => throw new NotImplementedException();
    
    /// <summary>
    /// Visits an integer.
    /// </summary>
    public virtual T Integer(IntegerNode node) => throw new NotImplementedException();
}