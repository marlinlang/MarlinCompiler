using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class MethodDeclarationNode : AstNode
{
    public string Name { get; }
    public bool IsStatic { get; }
    public MemberVisibility Visibility { get; }
    public MethodPrototypeNode Prototype { get; }

    public MethodDeclarationNode(ParserRuleContext context, string name, bool isStatic,
        MemberVisibility visibility, MethodPrototypeNode prototype) : base(context)
    {
        Name = name;
        IsStatic = isStatic;
        Visibility = visibility;
        Prototype = prototype;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitMethodDeclarationNode(this);
    }
}