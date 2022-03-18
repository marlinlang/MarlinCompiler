using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class ConstructorDeclarationNode : ContainerNode
{
    public ConstructorDeclarationNode(GetAccessibility accessibility, VariableNode[] parameters)
    {
        Accessibility = accessibility;
        Parameters = parameters;
    }
    
    public GetAccessibility Accessibility { get; }
    public VariableNode[] Parameters { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.ConstructorDeclaration(this);
    }
}