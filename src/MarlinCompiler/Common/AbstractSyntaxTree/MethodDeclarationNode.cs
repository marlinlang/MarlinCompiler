using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A method declaration.
/// </summary>
public class MethodDeclarationNode : ContainerNode
{
    public Accessibility Accessibility { get; }
    public TypeReferenceNode Type { get; }
    public string Name { get; }
    public VariableNode[] Args { get; }

    public MethodDeclarationNode(Accessibility accessibility, TypeReferenceNode type, string name, VariableNode[] args)
    {
        Accessibility = accessibility;
        Type = type;
        Name = name;
        Args = args;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.MethodDeclaration(this);
    }
}