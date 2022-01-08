using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class TypeReferenceNode : AstNode
{
    public string Name { get; set; }
    public bool IsArray { get; set; }

    public TypeReferenceNode(ParserRuleContext? context, string name) : base(context)
    {
        Name = name;
        IsArray = name.EndsWith("[]");
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitTypeReferenceNode(this);
    }
}