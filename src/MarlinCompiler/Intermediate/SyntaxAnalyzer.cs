using System.Text;
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
        RootSymbol root = new RootSymbol();
        _scope.Push(root);
        
        foreach (Pass pass in Enum.GetValues<Pass>())
        {
            _currentPass = pass;
            Visit(program.Children, root);
        }

        _scope.Pop();
    }
    
    #region Visitor methods
    
    public override Node MemberAccess(MemberAccessNode node)
    {
        // `this` keyword
        if (node.MemberName == "this" && node.Target == default)
        {
            TypeSymbol? type = (TypeSymbol?) _scope.Peek().Find(x => x is TypeSymbol);
            node.Symbol = new TypeInstanceSymbol(type);
            _scope.Peek().AddChild(node.Symbol);
        }
        else if (node.Target != default)
        {
            Visit(node.Target);

            node.Symbol = node.Target.Symbol?.FindInChildren(node.MemberName);
            if (node.Symbol is TypePropertySymbol var && !var.IsStatic)
            {
                node.Symbol = new TypeInstanceSymbol(var.Type);
                _scope.Peek().AddChild(node.Symbol);
            }
        }
        else
        {
            node.Symbol = _scope.Peek().Find(x => x is VariableSymbol && x.Name == node.MemberName);
        }

        return node;
    }

    public override Node ClassDefinition(ClassTypeDefinitionNode node)
    {
        if (_currentPass == Pass.MakeTypeSymbols)
        {
            node.Symbol = new ClassTypeSymbol(node.LocalName, node.ModuleName, node.Accessibility);
        }
        else
        {
            _scope.Push(node.Symbol!);

            if (node.BaseType != null)
            {
                ((ClassTypeSymbol) node.Symbol!).BaseClass = FindType(node.BaseType);
            }

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
                node.IsStatic,
                node.IsNative,
                node.GetAccessibility,
                node.SetAccessibility
            );
        }
        else if (_currentPass == Pass.VisitTypeMembers)
        {
            _scope.Push(node.Symbol!);
            
            if (node.Value != default)
            {
                ((TypePropertySymbol) node.Symbol!).Value = Visit(node.Value).Symbol;
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
            node.Symbol = new MethodSymbol(node.Name, node.IsStatic, (TypeSymbol?) node.Type.Symbol, args)
            {
                Signature = GetMethodSignature(node)
            };
            
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

    public override Node LocalVariable(LocalVariableDeclarationNode node)
    {
        Visit(node.Type);
        node.Symbol = new VariableSymbol(node.Name, (TypeSymbol?) node.Type.Symbol);
        
        _scope.Push(node.Symbol);

        if (node.Value != null)
        {
            ((VariableSymbol) node.Symbol!).Value = Visit(node.Value).Symbol;
        }

        _scope.Pop();

        return node;
    }

    public override Node MethodCall(MethodCallNode node)
    {
        // For methods, the parser inserts a `this` variable target if the
        // programmer didn't provide one, it's safe to imply non-nullability
        Visit(node.Target!);

        Symbol owner;
        bool isStatic = false;
        
        if (node.Target is MethodCallNode methodCall)
        {
            owner = new TypeInstanceSymbol(((MethodCallSymbol) methodCall.Symbol).Method?.Type);
        }
        else
        {
            owner = node.Target.Symbol;
            isStatic = node.Target is TypeReferenceNode;
        }
        
        string signature = GetMethodSignature(node);
        
        MethodSymbol? method = (MethodSymbol?) owner.Find(
            x => x is MethodSymbol m && m.Name == node.MethodName && m.Signature == signature
        );
        node.Symbol = new MethodCallSymbol(node.Target!.Symbol, method)
        {
            Signature = GetMethodSignature(node),
            StaticallyInvoked = isStatic
        };
        
        return node;
    }

    public override Node VariableAssignment(VariableAssignmentNode node)
    {
        VariableSymbol? var;
        if (node.Target != null)
        {
            Visit(node.Target);
            var = (VariableSymbol?) node.Target.Symbol?.FindInChildren(node.Name);
        }
        else
        {
            var = (VariableSymbol?) _scope.Peek().Find(
                x => x.FindInChildren(node.Name) != null
            )?.FindInChildren(node.Name);
        }

        node.Symbol = new VariableAssignmentSymbol(var, Visit(node.Value).Symbol!);
        
        return node;
    }

    public override Node Integer(IntegerNode node)
    {
        node.Symbol = new TypeInstanceSymbol(FindType("std:Integer"));
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
        return (TypeSymbol?) _scope.Peek().Find(x => x is TypeSymbol ty && $"{ty.Module}:{ty.Name}" == name);
    }

    /// <summary>
    /// Builds a method signature string by the args in the called method. 
    /// </summary>
    public static string GetMethodSignature(MethodCallNode call)
    {
        StringBuilder builder = new();

        foreach (ExpressionNode arg in call.Args)
        {
            builder.Append(arg switch
            {
                IntegerNode i => "std:Integer",
                // TODO: MemberAccessNode
                MethodCallNode mc => ((MethodCallSymbol?) mc.Symbol)?.Method?.Type?.Name ?? "<???>",
                _ => throw new NotImplementedException(arg.GetType().Name)
            });

            if (arg != call.Args.Last())
            {
                builder.Append(',');
            }
        }
        
        return builder.ToString();
    }

    /// <summary>
    /// Builds a method signature string by the args in the declared method. 
    /// </summary>
    public static string GetMethodSignature(MethodDeclarationNode declaration)
    {
        StringBuilder builder = new();

        foreach (VariableNode arg in declaration.Args)
        {
            builder.Append(arg.Type.Symbol?.Name ?? "<???>");

            if (arg != declaration.Args.Last())
            {
                builder.Append(',');
            }
        }
        
        return builder.ToString();
    }
    
    #endregion
}