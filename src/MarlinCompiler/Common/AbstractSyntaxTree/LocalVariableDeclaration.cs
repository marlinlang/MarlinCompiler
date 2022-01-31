using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Local variable declaration.
/// </summary>
public class LocalVariableDeclaration : VariableNode
{
    public LocalVariableDeclaration(TypeReferenceNode type, string name, Node? value)
        : base(type, name, value)
    {
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.LocalVariable(this);
    }
}