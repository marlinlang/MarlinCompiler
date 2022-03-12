using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class ConstructorDeclarationNode : ContainerNode
{
    public GetAccessibility Accessibility { get; }
    public VariableNode[] Parameters { get; }

    public ConstructorDeclarationNode(GetAccessibility accessibility, VariableNode[] parameters)
    {
        Accessibility = accessibility;
        Parameters = parameters;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.ConstructorDeclaration(this);
    }
}