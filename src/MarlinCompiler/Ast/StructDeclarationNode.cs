using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public sealed class StructDeclarationNode : TypeDeclarationNode
{
    public StructDeclarationNode(ParserRuleContext context, string name, MemberVisibility visibility)
        : base(context, name, visibility)
    {
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitStructDeclarationNode(this);
    }
}