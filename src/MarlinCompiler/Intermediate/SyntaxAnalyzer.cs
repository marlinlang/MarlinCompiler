using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Intermediate;

/// <summary>
/// Syntax analyzer.
/// </summary>
public sealed class SyntaxAnalyzer : BaseAstVisitor<Node>
{
    public MessageCollection MessageCollection;

    /// <summary>
    /// The current pass of the analyzer.
    /// </summary>
    private Pass _currentPass;

    /// <summary>
    /// The scope state of the analyzer.
    /// </summary>
    private Stack<Symbol> _scope;

    /// <summary>
    /// Describes the current analyzer pass. The analyzer does different operations based
    /// on its current pass.
    /// </summary>
    private enum Pass
    {
        /// <summary>
        /// Create symbols for types.
        /// </summary>
        MakeTypeSymbols = 1,
        
        /// <summary>
        /// Create symbols for properties and methods.
        /// </summary>
        MakeMembers = 2,
        
        /// <summary>
        /// Evaluate properties and go inside of methods.
        /// </summary>
        VisitTypeMembers = 3
    }
    
    public SyntaxAnalyzer()
    {
        MessageCollection = new MessageCollection();
        _scope = new Stack<Symbol>();
    }

    /// <summary>
    /// Analyzes a program.
    /// </summary>
    public void Analyze(ContainerNode program)
    {
        foreach (Pass pass in Enum.GetValues<Pass>())
        {
            _currentPass = pass;
            Visit(program);
        }
    }
    
    #region Visitor methods
    
    public override Node MemberAccess(MemberAccessNode node)
    {
        throw new NotImplementedException();
    }

    public override Node ClassDefinition(ClassTypeDefinitionNode node)
    {
        if (_currentPass == Pass.MakeTypeSymbols)
        {
            node.Symbol = new ClassTypeSymbol(node.LocalName, node.ModuleName, node.Accessibility)
            {
                BaseClass = node.BaseType
            };
        }
        else
        {
            _scope.Push(node.Symbol!);
            Visit(node.Children, node.Symbol);
            _scope.Pop();
        }

        return node;
    }

    public override Node StructDefinition(StructTypeDefinitionNode node)
    {
        if (_currentPass == Pass.MakeTypeSymbols)
        {
            node.Symbol = new StructTypeSymbol(node.LocalName, node.ModuleName, node.Accessibility);
        }
        else
        {
            _scope.Push(node.Symbol!);
            Visit(node.Children, node.Symbol);
            _scope.Pop();
        }

        return node;
    }

    public override Node Property(PropertyNode node)
    {
        if (_currentPass == Pass.MakeMembers)
        {
            Visit(node.Type);
            
            node.Symbol = new TypePropertySymbol(
                node.Name, 
                (TypeSymbol?) Visit(node.Type).Symbol,
                node.GetAccessibility,
                node.SetAccessibility
            );
        }
        else if (_currentPass == Pass.VisitTypeMembers)
        {
            _scope.Push(node.Symbol!);
            
            if (node.Value != default)
            {
                ((TypePropertySymbol) node.Symbol!).Value = Visit(node.Value)!.Symbol;
            }
            _scope.Pop();
        }

        return node;
    }

    public override Node MethodDeclaration(MethodDeclarationNode node)
    {
        if (_currentPass == Pass.MakeMembers)
        {
            List<VariableSymbol> args = new();
            foreach (VariableNode arg in node.Args)
            {
                Visit(arg.Type);
                arg.Symbol = new VariableSymbol(arg.Name, (TypeSymbol?) arg.Type.Symbol);
                args.Add((VariableSymbol) arg.Symbol);
            }

            Visit(node.Type);
            node.Symbol = new MethodSymbol(node.Name, (TypeSymbol?) node.Type.Symbol, args);
            
            foreach (VariableNode arg in node.Args)
            {
                node.Symbol.AddChild(arg.Symbol!);
            }
        }
        else if (_currentPass == Pass.VisitTypeMembers)
        {
            _scope.Push(node.Symbol!);

            Visit(node.Children, node.Symbol);
            
            _scope.Pop();
        }

        return node;
    }

    public override Node LocalVariable(LocalVariableDeclaration node)
    {
        throw new NotImplementedException();
    }

    public override Node MethodCall(MethodCallNode node)
    {
        throw new NotImplementedException();
    }

    public override Node VariableAssignment(VariableAssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public override Node Tuple(TupleNode node)
    {
        throw new NotImplementedException();
    }

    public override Node Integer(IntegerNode node)
    {
        node.Symbol = new TypeInstanceSymbol(FindType("std.Integer"));
        return node;
    }

    public override Node TypeReference(TypeReferenceNode node)
    {
        node.Symbol = FindType(node.FullName);

        return node;
    }
    
    #endregion

    #region Utilities

    /// <summary>
    /// Utility method for visiting the children of a node.
    /// </summary>
    private void Visit(List<Node> children, Symbol? setParent)
    {
        children.ForEach(child =>
        {
            child.AcceptVisitor(this);
            
            if (child.Symbol != default && setParent != default)
            {
                setParent.AddChild(child.Symbol);
            }
        });
    }

    /// <summary>
    /// Looks for a type in the current scope.
    /// </summary>
    /// <param name="name">The full name of the type, incl. module</param>
    private TypeSymbol? FindType(string name)
    {
        return (TypeSymbol?) _scope.Peek().Lookup(x => x is TypeSymbol ty && $"{ty.Module}.{ty.Name}" == name);
    }
    
    #endregion
}