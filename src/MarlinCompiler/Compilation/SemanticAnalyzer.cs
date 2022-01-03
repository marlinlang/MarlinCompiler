using System.Text;
using MarlinCompiler.Ast;
using MarlinCompiler.Symbols;

namespace MarlinCompiler.Compilation;

internal class SemanticAnalyzer : BaseAstVisitor<AstNode>
{
    public CompileMessages Messages { get; }
    
    private readonly Builder _builder;
    private readonly Stack<AstNode> _contextStack;

    public SemanticAnalyzer(Builder builder)
    {
        _builder = builder;
        _contextStack = new Stack<AstNode>();
    }

    public override AstNode VisitBlockNode(BlockNode node)
    {
        if (node is RootBlockNode)
        {
            node.Symbol = new RootSymbol();
            
            _contextStack.Push(node);
        }
        
        VisitChildren(node);

        if (node is RootBlockNode)
        {
            _contextStack.Pop();
        }
        
        return node;
    }

    public override AstNode VisitClassDeclarationNode(ClassDeclarationNode node)
    {
        _contextStack.Push(node);
        
        VisitChildren(node);
        
        _contextStack.Pop();
        
        return node;
    }

    public override AstNode VisitMemberAccessNode(MemberAccessNode node)
    {
        AstNode currentParent = node.Parent != null ? Visit(node.Parent) : _contextStack.Peek();
        AstNode current = Visit(node.Member);
        while (true)
        {
            switch (current)
            {
                case NameReferenceNode nameRef:
                    node.Symbol = currentParent.Symbol.Lookup(nameRef.Name);
                    return node;
                
                case MemberAccessNode memberAccess:
                    currentParent = current;
                    current = memberAccess;
                    continue;
                
                default:
                    throw new NotImplementedException(current.GetType().Name);
            }
        }
    }
    
    public override AstNode VisitMethodDeclarationNode(MethodDeclarationNode node)
    {
        VisitMethodPrototypeNode(node.Prototype);
        
        return node;
    }

    public override AstNode VisitMethodPrototypeNode(MethodPrototypeNode node)
    {
        _contextStack.Push(node);
        
        VisitChildren(node);
        
        _contextStack.Pop();
        
        return node;
    }

    public override AstNode VisitMethodCallNode(MethodCallNode node)
    {
        node.Symbol = VisitMemberAccessNode(node.Member).Symbol;
        return node;
    }

    public override AstNode VisitReturnNode(ReturnNode node)
    {
        if (node.Value != null) Visit(node.Value);
        
        return node;
    }

    public override AstNode VisitVariableAssignmentNode(VariableAssignmentNode node)
    {
        Visit(node.Member);
        Visit(node.Value);

        node.Symbol = node.Member.Symbol;
        
        return node;
    }

    public override AstNode VisitVariableDeclarationNode(VariableDeclarationNode node)
    {
        if (_contextStack.Peek().Symbol.Lookup(node.Name) != null)
        {
            Messages.AddError(
                $"Name {node.Name} already used somewhere else", 
                new FileLocation(_builder, node.Context.Start)
            );
        }

        node.Symbol = new VariableSymbol(node.Name, node.Type.Name);

        if (node.Value != null) Visit(node.Value);
        
        return node;
    }

    public override TypeReferenceNode VisitTypeReferenceNode(TypeReferenceNode node)
    {
        node.Symbol = _contextStack.Peek().Symbol.Lookup(node.Name);
        
        return node;
    }

    public override AstNode VisitNameReferenceNode(NameReferenceNode node)
    {
        return node;
    }
}