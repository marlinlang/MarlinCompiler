using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class TypeDeclarationNode : AstNode
{
    public string Name { get; set; }
    public MemberVisibility Visibility { get; }
    public BlockNode TypeBody { get; }
    public override IEnumerable<AstNode> Children => TypeBody.Body;

    public TypeDeclarationNode(ParserRuleContext context, string name, MemberVisibility visibility) : base(context)
    {
        Name = name;
        Visibility = visibility;
        TypeBody = new BlockNode(context);
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        throw new InvalidOperationException($"Base class {nameof(TypeDeclarationNode)} cannot accept visitors.");
    }
}