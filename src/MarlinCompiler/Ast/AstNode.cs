using Antlr4.Runtime;
using MarlinCompiler.Antlr;
using MarlinCompiler.Symbols;

namespace MarlinCompiler.Ast;

public abstract class AstNode
{
    public ParserRuleContext Context { get; }
    public virtual IEnumerable<AstNode> Children { get; set; }
    
    public Symbol Symbol { get; set; }

    public AstNode(ParserRuleContext context)
    {
        Context = context;
        Children = new List<AstNode>();
    }

    public abstract TResult Accept<TResult>(IAstVisitor<TResult> visitor);
}