using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class MethodDeclarationNode : AstNode
{
    public string Name { get; }
    public bool IsStatic { get; }
    public MemberVisibility Visibility { get; }
    public MethodPrototypeNode Prototype { get; }
    public TypeReferenceNode Type { get; }

    public MethodDeclarationNode(ParserRuleContext context, string name, bool isStatic,
        MemberVisibility visibility, MethodPrototypeNode prototype, TypeReferenceNode type) : base(context)
    {
        Name = name;
        IsStatic = isStatic;
        Visibility = visibility;
        Prototype = prototype;
        Type = type;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitMethodDeclarationNode(this);
    }
}