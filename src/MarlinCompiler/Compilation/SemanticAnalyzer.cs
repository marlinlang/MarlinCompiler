using System.Text;
using MarlinCompiler.Antlr;
using MarlinCompiler.Ast;
using MarlinCompiler.MarlinCompiler.Compilation;
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

    public override AstNode VisitStructDeclarationNode(StructDeclarationNode node)
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
            if (currentParent == null || current == null
                || currentParent.Symbol == null)
            {
                return null;
            }
            
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

        ((MethodSymbol) node.Symbol).Type = (TypeSymbol) VisitTypeReferenceNode(node.Type).Symbol;
        
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
        if (!node.IsNative)
        {
            MethodSymbol? found = null;
            Symbol initial = VisitMemberAccessNode(node.Member)?.Symbol;

            if (initial != null)
            {
                Symbol owner = VisitMemberAccessNode(node.Member).Symbol.Parent;

                foreach (Symbol tryFind in owner.LookupMultiple(initial.Name))
                {
                    if (tryFind is MethodSymbol tryMethod)
                    {
                        if (tryMethod.Args.Count == node.Args.Count)
                        {
                            for (int i = 0; i < tryMethod.Args.Count; i++)
                            {
                                string expected = ((VariableSymbol) tryMethod.Args[i]).Type;
                                string given = SemanticUtils.GetNodeTypeName(Visit(node.Args[i]));

                                TypeSymbol expectedType = (TypeSymbol) _contextStack.Peek().Symbol.Lookup(expected);
                                TypeSymbol givenType = (TypeSymbol) _contextStack.Peek().Symbol.Lookup(given);

                                if (SemanticUtils.AreTypesCompatible(expectedType, givenType))
                                {
                                    found = tryMethod;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // properties go at the top so this will be detected instantly
                        Messages.Error(
                            $"Cannot call non-method {tryFind.Name} (a {tryFind.UserType})",
                            new FileLocation(
                                _builder,
                                ((MarlinParser.MethodCallContext) node.Context).memberAccess().Stop
                            )
                        );
                    }
                }
            }

            if (found != null)
                node.Symbol = found;
        }

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

        VisitTypeReferenceNode(node.Type);
        if (node.Value != null) Visit(node.Value);
        
        return node;
    }

    public override TypeReferenceNode VisitTypeReferenceNode(TypeReferenceNode node)
    {
        string useName = node.IsArray ? node.Name[..^2] : node.Name;
        
        node.Symbol = _contextStack.Peek().Symbol.Lookup(useName);
        
        return node;
    }

    public override AstNode VisitArrayInitializerNode(ArrayInitializerNode node)
    {
        Visit(node.ArrayType);

        return node;
    }

    public override AstNode VisitNameReferenceNode(NameReferenceNode node) => node;

    public override AstNode VisitBooleanNode(BooleanNode node) => node;

    public override AstNode VisitDoubleNode(DoubleNode node) => node;

    public override AstNode VisitIntegerNode(IntegerNode node) => node;

    public override AstNode VisitStringNode(StringNode node) => node;

    public override AstNode VisitCharacterNode(CharacterNode node) => node;
}