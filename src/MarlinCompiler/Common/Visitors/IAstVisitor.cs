using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Visitors;

/// <summary>
/// A visitor that supports all AST nodes.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAstVisitor<T>
{
    public T Visit(Node node) => node.AcceptVisitor(this);

    /// <summary>
    /// Visits a compiler inserted placeholder.
    /// </summary>
    public virtual T CompilerInsertedPlaceholder(CompilerInsertedPlaceholderNode node) => default;
    
    /// <summary>
    /// Visits a member access.
    /// </summary>
    /// <remarks>Keep in mind that the left side might be a <see cref="CompilerInsertedPlaceholderNode"/></remarks>
    public virtual T MemberAccess(MemberAccessNode node) => default;
    
    /// <summary>
    /// Visits a class declaration.
    /// </summary>
    public virtual T ClassDefinition(ClassTypeDefinitionNode node) => default;
    
    /// <summary>
    /// Visits a type reference.
    /// </summary>
    public virtual T TypeReference(TypeReferenceNode node) => default;
    
    /// <summary>
    /// Visits type property members.
    /// </summary>
    public virtual T Property(PropertyNode node) => default;

    /// <summary>
    /// Visits a method declaration.
    /// </summary>
    public virtual T MethodDeclaration(MethodDeclarationNode node) => default;

    /// <summary>
    /// Visits a local variable.
    /// </summary>
    public virtual T LocalVariable(LocalVariableDeclaration node) => default;
}