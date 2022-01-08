using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public sealed class CharacterNode : AstNode
{
    public char Value { get; }

    public CharacterNode(ParserRuleContext? context, char value) : base(context)
    {
        Value = value;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitCharacterNode(this);
    }
}