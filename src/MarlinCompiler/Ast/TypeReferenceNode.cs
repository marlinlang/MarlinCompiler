using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class TypeReferenceNode : AstNode
{
    public string Name { get; set; }
    public bool IsArray { get; }

    public TypeReferenceNode(ParserRuleContext? context, string name) : base(context)
    {
        Name = name;
        IsArray = Name.EndsWith("[]");
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitTypeReferenceNode(this);
    }
}