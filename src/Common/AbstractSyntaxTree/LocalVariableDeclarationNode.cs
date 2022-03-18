using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Local variable declaration.
/// </summary>
public class LocalVariableDeclarationNode : VariableNode
{
    public LocalVariableDeclarationNode(TypeReferenceNode type, string name, bool mutable, ExpressionNode? value)
        : base(type, name, value)
    {
        Mutable = mutable;
    }
    
    public bool Mutable { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.LocalVariable(this);
    }
}