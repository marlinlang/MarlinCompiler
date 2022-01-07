using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class VariableDeclarationNode : AstNode
{
    public TypeReferenceNode Type { get; }
    public string Name { get; }
    public AstNode? Value { get; }
    public bool IsStatic { get; }
    public bool IsNative { get; }
    public MemberVisibility Visibility { get; }

    public VariableDeclarationNode(ParserRuleContext context, TypeReferenceNode type, string name,
        AstNode? value, bool isStatic, bool isNative, MemberVisibility visibility) : base(context)
    {
        Type = type;
        Name = name;
        Value = value;
        IsStatic = isStatic;
        IsNative = isNative;
        Visibility = visibility;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitVariableDeclarationNode(this);
    }
}