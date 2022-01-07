namespace MarlinCompiler.Ast;

/// <summary>
/// Utility class to create partial AST visitors.
/// </summary>
public abstract class BaseAstVisitor<TResult> : IAstVisitor<TResult>
{
    public TResult Visit(AstNode node)
    {
        return node.Accept(this);
    }

    public virtual void VisitChildren(AstNode node)
    {
        foreach (AstNode child in node.Children)
        {
            Visit(child);
            if (node.Symbol != null
                && child.Symbol != null
                && child.Symbol?.Parent != node.Symbol)
            {
                node.Symbol.AddChild(child.Symbol);
            }
        }
    }

    public virtual TResult VisitBlockNode(BlockNode node)
    { 
        VisitChildren(node);
        return default;
    }
    
    public virtual TResult VisitBooleanNode(BooleanNode node)
    { 
        VisitChildren(node);
        return default;
    }
    
    public virtual TResult VisitClassDeclarationNode(ClassDeclarationNode node)
    { 
        VisitChildren(node);
        return default;
    }

    public virtual TResult VisitStructDeclarationNode(StructDeclarationNode node)
    {
        VisitChildren(node);
        return default;
    }

    public virtual TResult VisitDoubleNode(DoubleNode node)
    { 
        VisitChildren(node);
        return default;
    }

    public virtual TResult VisitIntegerNode(IntegerNode node)
    { 
        VisitChildren(node);
        return default;
    }

    public virtual TResult VisitMemberAccessNode(MemberAccessNode node)
    { 
        VisitChildren(node);
        return default;
    }

    public virtual TResult VisitMethodDeclarationNode(MethodDeclarationNode node)
    { 
        VisitChildren(node);
        return default;
    }

    public virtual TResult VisitMethodPrototypeNode(MethodPrototypeNode node)
    { 
        VisitChildren(node);
        return default;
    }
    
    public virtual TResult VisitMethodCallNode(MethodCallNode node)
    { 
        VisitChildren(node);
        return default;
    }
    
    public virtual TResult VisitReturnNode(ReturnNode node)
    { 
        VisitChildren(node);
        return default;
    }
    
    public virtual TResult VisitStringNode(StringNode node)
    { 
        VisitChildren(node);
        return default;
    }
    
    public virtual TResult VisitVariableAssignmentNode(VariableAssignmentNode node)
    { 
        VisitChildren(node);
        return default;
    }
    
    public virtual TResult VisitVariableDeclarationNode(VariableDeclarationNode node)
    { 
        VisitChildren(node);
        return default;
    }
    
    public virtual TResult VisitTypeReferenceNode(TypeReferenceNode node)
    { 
        VisitChildren(node);
        return default;
    }
    
    public virtual TResult VisitNameReferenceNode(NameReferenceNode node)
    { 
        VisitChildren(node);
        return default;
    }

    public virtual TResult VisitArrayInitializerNode(ArrayInitializerNode node)
    {
        VisitChildren(node);
        return default;
    }
}
