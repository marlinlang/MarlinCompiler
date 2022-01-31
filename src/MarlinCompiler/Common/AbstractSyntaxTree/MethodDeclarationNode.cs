using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A method declaration.
/// </summary>
public class MethodDeclarationNode : ContainerNode
{
    public GetAccessibility Accessibility { get; }
    public TypeReferenceNode Type { get; }
    public string Name { get; }
    public bool IsStatic { get; }
    public VariableNode[] Args { get; }

    public MethodDeclarationNode(GetAccessibility accessibility, TypeReferenceNode type, string name,
        bool isStatic, VariableNode[] args)
    {
        Accessibility = accessibility;
        Type = type;
        Name = name;
        IsStatic = isStatic;
        Args = args;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.MethodDeclaration(this);
    }
}