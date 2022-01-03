using System.Text;
using MarlinCompiler.Antlr;
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
        Messages = new CompileMessages();
        _builder = builder;
        _contextStack = new Stack<AstNode>();
    }

    public override AstNode VisitBlockNode(BlockNode node)
    {
        if (node is RootBlockNode)
        {
            if (node.Symbol == null) { node.Symbol = new RootSymbol(); }

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

        foreach (ArgumentVariableDeclarationNode arg in node.Args)
        {
            Visit(arg.Type);
        }
        
        VisitChildren(node);
        
        _contextStack.Pop();
        
        return node;
    }

    public override AstNode VisitMethodCallNode(MethodCallNode node)
    {
        Symbol initial = VisitMemberAccessNode(node.Member).Symbol;
        Symbol owner = VisitMemberAccessNode(node.Member).Symbol.Parent;

        MethodSymbol found = null;
        
        StringBuilder givenArgsSb = new();
        foreach (VariableDeclarationNode arg in node.Args)
        {
            givenArgsSb.Append(arg.Type.Name).Append('-');
        }

        string givenArgs = givenArgsSb.ToString();
        
        foreach (Symbol tryFind in owner.LookupMultiple(initial.Name))
        {
            if (tryFind is MethodSymbol tryMethod)
            {
                StringBuilder expectedArgsSb = new();
                foreach (VariableSymbol arg in tryMethod.Args)
                {
                    expectedArgsSb.Append(arg.Type).Append('-');
                }

                if (expectedArgsSb.ToString() == givenArgs)
                {
                    found = tryMethod;
                    break;
                }
            }
            else
            {
                // properties go at the top so this will be detected instantly
                Messages.Error(
                    $"Cannot call non-method {tryFind.Name}",
                    new FileLocation(
                        _builder,
                        ((MarlinParser.MethodCallContext) node.Context).memberAccess().Stop
                    )
                );
            }
        }

        node.Symbol = found;
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
            Messages.Error(
                $"Name {node.Name} already used somewhere else", 
                new FileLocation(_builder, node.Context.Start)
            );
        }

        node.Symbol = new VariableSymbol(node.Name, node.Type.Name);

        Visit(node.Type);
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