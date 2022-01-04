using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public class ArrayInitializerNode : AstNode
{
    public TypeReferenceNode ArrayType { get; }
    public AstNode ElementCount { get; }
    public AstNode[] InitialElements { get; }

    public override IEnumerable<AstNode> Children
    {
        get
        {
            yield return ElementCount;
            foreach (AstNode n in InitialElements)
            {
                yield return n;
            }
        }
    }

    public ArrayInitializerNode(ParserRuleContext context, TypeReferenceNode arrayType, AstNode elementCount,
        AstNode[] initialElements) : base(context)
    {
        ArrayType = arrayType;
        ElementCount = elementCount;
        InitialElements = initialElements;
    }

    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
    {
        return visitor.VisitArrayInitializerNode(this);
    }
}