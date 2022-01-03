using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class MethodPrototypeNode : AstNode
{
    public List<ArgumentVariableDeclarationNode> Args { get; }
    public List<AstNode> Body { get; }

    public override IEnumerable<AstNode> Children => Body;

    public MethodPrototypeNode(ParserRuleContext context, List<ArgumentVariableDeclarationNode> args,
        List<AstNode> body) : base(context)
    {
        Args = args;
        Body = body;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitMethodPrototypeNode(this);
    }
}