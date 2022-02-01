using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Intermediate;

/// <summary>
/// Checks the program for logical errors.
/// </summary>
public class SemanticChecker : BaseAstVisitor<Node>
{
    /// <summary>
    /// The current pass of the analyzer.
    /// </summary>
    public MessageCollection MessageCollection;

    public SemanticChecker()
    {
        MessageCollection = new MessageCollection();
    }
    
    #region Visitor
    
    public override Node MemberAccess(MemberAccessNode node)
    {
        if (node.Symbol == default)
        {
            MessageCollection.Error($"Cannot find member {node.MemberName}", node.Location);
        }

        // `this` keyword
        if (node.MemberName == "this" && node.Target == default)
        {
            MethodSymbol symbol = (MethodSymbol) node.Symbol!.Find(x => x is MethodSymbol)!;
            if (symbol.IsStatic)
            {
                MessageCollection.Error("Attempt to access instance data in static method", node.Location);
            }
        }
        
        return node;
    }

    public override Node ClassDefinition(ClassTypeDefinitionNode node)
    {
        if (node.Symbol!.FindAll(
                x => x is TypeSymbol t && t.Module == node.ModuleName && t.Name == node.LocalName).Length > 1
            )
        {
            MessageCollection.Error($"Repeated definition of type {node.Symbol.Name}", node.Location);
        }

        Visit(node.Children);
        
        return node;
    }

    public override Node StructDefinition(StructTypeDefinitionNode node)
    {
        if (node.Symbol!.FindAll(
                x => x is TypeSymbol t && t.Module == node.ModuleName && t.Name == node.LocalName).Length > 1
           )
        {
            MessageCollection.Error($"Repeated definition of type {node.Symbol.Name}", node.Location);
        }

        Visit(node.Children);

        return node;
    }

    public override Node TypeReference(TypeReferenceNode node)
    {
        if (node.Symbol == default)
        {
            MessageCollection.Error($"Unknown type {node.FullName}", node.Location);
        }
        
        return node;
    }

    public override Node Property(PropertyNode node)
    {
        //TODO
        return node;
    }

    public override Node MethodDeclaration(MethodDeclarationNode node)
    {
        Visit(node.Type);

        Visit(node.Children);
        
        return node;
    }

    public override Node LocalVariable(LocalVariableDeclarationNode node)
    {
        return base.LocalVariable(node);
    }

    public override Node MethodCall(MethodCallNode node)
    {
        if (node.Target != null)
        {
            Visit(node.Target);
        }
        
        MethodCallSymbol symbol = (MethodCallSymbol) node.Symbol!;
        
        if (symbol.Method == default)
        {
            MessageCollection.Error($"Cannot find method {node.MethodName}({symbol.Signature})", node.Location);
        }
        else
        {
            bool staticCall = false;

            if (!symbol.Method.IsStatic && symbol.StaticallyInvoked)
            {
                MessageCollection.Error(
                    $"Cannot call instance method {node.MethodName} statically. Use an instance",
                    node.Location
                );
            }
            else if (symbol.Method.IsStatic && !symbol.StaticallyInvoked)
            {
                MessageCollection.Error(
                    $"Cannot call static method {node.MethodName} by instance. Use the type name.",
                    node.Location
                );
            }
        }

        return node;
    }

    public override Node VariableAssignment(VariableAssignmentNode node)
    {
        // TODO
        
        return node;
    }
    
    #endregion
    
    #region Utilities
    
    /// <summary>
    /// Utility method for visiting the children of a node.
    /// </summary>
    private void Visit(List<Node> children)
    {
        children.ForEach(child => child.AcceptVisitor(this));
    }
    
    #endregion
}