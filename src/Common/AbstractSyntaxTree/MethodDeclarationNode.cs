using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A method declaration.
/// </summary>
public class MethodDeclarationNode : ContainerNode
{
    public MethodDeclarationNode(GetAccessibility accessibility, TypeReferenceNode type, string name,
        bool isStatic, VariableNode[] parameters)
    {
        Accessibility = accessibility;
        Type = type;
        Name = name;
        IsStatic = isStatic;
        Parameters = parameters;
    }
    
    public GetAccessibility Accessibility { get; }
    public TypeReferenceNode Type { get; }
    public string Name { get; }
    public bool IsStatic { get; }
    public VariableNode[] Parameters { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.MethodDeclaration(this);
    }
}