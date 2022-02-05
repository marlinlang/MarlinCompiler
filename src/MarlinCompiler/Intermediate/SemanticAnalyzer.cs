using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Semantics.Symbols;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Intermediate;

public sealed class SemanticAnalyzer : IAstVisitor<Node>
{
    private enum Pass
    {
        /// <summary>
        /// Create type declaration symbols.
        /// </summary>
        InitializeTypes = 1,
        
        /// <summary>
        /// Create method and property symbols.
        /// For subclasses, find the base class symbol and store it.
        /// </summary>
        InitializeTypeMembers = 2,
        
        /// <summary>
        /// Visit property values and method bodies.
        /// </summary>
        VisitTypeMembers = 3,
    }
    
    public MessageCollection MessageCollection { get; }
    private Pass _currentPass;
    private readonly Stack<Symbol> _context;

    public SemanticAnalyzer()
    {
        MessageCollection = new MessageCollection();
        _context = new Stack<Symbol>();
    }

    public void Analyze(ContainerNode root)
    {
        Symbol rootSymbol = new Symbol();
        _context.Push(rootSymbol);
        foreach (Pass pass in Enum.GetValues<Pass>())
        {
            _currentPass = pass;
            VisitChildrenAndParent(root.Children, rootSymbol);
        }

        if (_context.Pop() != rootSymbol)
        {
            throw new InvalidOperationException("Stack isn't popped");
        }
    }

    public Node Visit(Node node) => node.AcceptVisitor(this);

    public Node MemberAccess(MemberAccessNode node)
    {
        // handling for `this`
        if (node.Target == null && node.MemberName == "this")
        {
            TypeDeclarationSymbol? type =
                (TypeDeclarationSymbol?) _context.Peek().FindParent(x => x is TypeDeclarationSymbol);
            node.MemberSymbol = new TypeReferenceSymbol(type, null, false);
            return node;
        }
        
        Symbol? owner = null;
        
        if (node.Target != null)
        {
            Visit(node.Target);

            owner = ResolveIndexableExpressionTarget(node);
        }
        else
        {
            owner = _context.Peek();
        }

        if (owner == null)
        {
            MessageCollection.Error($"Cannot find owner of member {node.MemberName}", node.Location);
        }
        else
        {
            bool staticAccess = node.Target is TypeReferenceNode;
            if (owner is TypeReferenceSymbol tRef) { owner = tRef.Type; }
            
            Symbol? member = owner?.Search(
                x => x is MethodDeclarationSymbol method && method.Name == node.MemberName
                     || x is VariableSymbol var && var.Name == node.MemberName
            );

            node.MemberSymbol = member;

            if (member == null)
            {
                MessageCollection.Error($"Unknown member {node.MemberName}", node.Location);
            }
            else if (member is MethodDeclarationSymbol method)
            {
                // Don't allow static access of method name without instance
                // NB: variables and method calls have typeref, not typedecl symbols
                // If the owner is a typedecl, that means its the actual type itself
                if (!method.IsStatic && staticAccess)
                {
                    MessageCollection.Error("Cannot access non-static method statically", node.Location);
                }
                else if (method.IsStatic && !staticAccess)
                {
                    // Opposite: we tried to access a static method on a non-static value instead of the type itself
                    MessageCollection.Error("Cannot access static method by instance", node.Location);
                }
            }
            else if (member is VariableSymbol var)
            {
                // For properties, do the same as for methods
                if (var is PropertyVariableSymbol prop)
                {
                    if (!prop.IsStatic && staticAccess)
                    {
                        MessageCollection.Error("Cannot access non-static property statically", node.Location);
                    }
                    else if (prop.IsStatic && !staticAccess)
                    {
                        MessageCollection.Error("Cannot access static property by instance", node.Location);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        return node;
    }

    public Node ClassDefinition(ClassTypeDefinitionNode node)
    {
        switch (_currentPass)
        {
            case Pass.InitializeTypes:
            {
                node.DeclarationSymbol = new ClassTypeDeclarationSymbol(
                    $"{node.ModuleName}::{node.LocalName}",
                    node.Accessibility,
                    node.IsStatic
                );
                break;
            }
            
            case Pass.InitializeTypeMembers:
            {
                // Base class reference
                if (node.BaseType != null)
                {
                    Visit(node.BaseType);

                    TypeReferenceSymbol baseType = node.BaseType.TypeSymbol!;

                    if (baseType.Type == null)
                    {
                        MessageCollection.Error($"Unknown base class {node.BaseType.FullName}", node.Location);
                    }
                    else if (baseType.Type is not ClassTypeDeclarationSymbol)
                    {
                        MessageCollection.Error($"Cannot inherit from non-class {baseType.Type.Name}", node.Location);
                    }
                    else
                    {
                        // All good!
                        node.BaseTypeSymbol = baseType;
                    }
                }
                
                _context.Push(node.DeclarationSymbol!);
                VisitChildrenAndParent(node.Children, node.DeclarationSymbol);
                _context.Pop();

                break;
            }
            
            case Pass.VisitTypeMembers:
            {
                _context.Push(node.DeclarationSymbol!);
                VisitChildrenAndParent(node.Children, node.DeclarationSymbol);
                _context.Pop();
                break;
            }
        }

        return node;
    }

    public Node StructDefinition(StructTypeDefinitionNode node)
    {
        switch (_currentPass)
        {
            case Pass.InitializeTypes:
            {
                node.DeclarationSymbol = new StructTypeDeclarationSymbol(
                    $"{node.ModuleName}::{node.LocalName}",
                    node.Accessibility
                );
                break;
            }
            
            case Pass.InitializeTypeMembers:
            case Pass.VisitTypeMembers:
            {
                _context.Push(node.DeclarationSymbol!);
                VisitChildrenAndParent(node.Children, node.DeclarationSymbol);
                _context.Pop();
                break;
            }
        }

        return node;
    }

    public Node TypeReference(TypeReferenceNode node)
    {
        // TODO: Generics
        
        node.TypeSymbol = new TypeReferenceSymbol(
            (TypeDeclarationSymbol?) _context.Peek().Search(
                x => x is TypeDeclarationSymbol ty && ty.Name == node.FullName
            ),
            null,
            node.IsArray
        );

        if (node.FullName == "std::Object" && node.TypeSymbol.Type == null)
        {
            ;
        }

        if (node.TypeSymbol == null)
        {
            MessageCollection.Error($"Unknown type {node.FullName}", node.Location);
        }

        return node;
    }

    public Node Property(PropertyNode node)
    {
        switch (_currentPass)
        {
            case Pass.InitializeTypeMembers:
            {
                node.Symbol = new PropertyVariableSymbol(
                    ((TypeReferenceNode) Visit(node.Type)).TypeSymbol,
                    node.Name,
                    node.IsStatic,
                    node.IsNative,
                    node.GetAccessibility,
                    node.SetAccessibility
                );
                break;
            }
            case Pass.VisitTypeMembers:
            {
                if (node.Value != null)
                {
                    _context.Push(node.Symbol!);
                    Visit(node.Value);
                    _context.Pop();
                }

                break;
            }
        }

        return node;
    }

    public Node MethodDeclaration(MethodDeclarationNode node)
    {
        switch (_currentPass)
        {
            case Pass.InitializeTypeMembers:
            {
                node.DeclarationSymbol = new MethodDeclarationSymbol(
                    node.Type.TypeSymbol,
                    node.Accessibility,
                    node.Name,
                    node.IsStatic
                );

                List<string> signatureList = new();
                foreach (VariableNode arg in node.Args)
                {
                    signatureList.Add(arg.Type.FullName);
                }
                node.DeclarationSymbol.Signature = String.Join(',', signatureList);

                node.ReturnTypeSymbol = ((TypeReferenceNode) Visit(node.Type)).TypeSymbol;

                foreach (VariableNode arg in node.Args)
                {
                    arg.Symbol = new VariableSymbol(
                        ((TypeReferenceNode) Visit(arg.Type)).TypeSymbol,
                        arg.Name
                    );
                    
                    node.DeclarationSymbol.AddChild(arg.Symbol);
                }
                
                break;
            }

            case Pass.VisitTypeMembers:
            {
                _context.Push(node.DeclarationSymbol!);
                VisitChildrenAndParent(node.Children, node.DeclarationSymbol);
                _context.Pop();
                break;
            }
        }

        return node;
    }

    public Node ConstructorDeclaration(ConstructorDeclarationNode node)
    {
        switch (_currentPass)
        {
            case Pass.InitializeTypeMembers:
            {
                node.DeclarationSymbol = new MethodDeclarationSymbol(
                    (TypeReferenceSymbol?) _context.Peek().Search(x => x is TypeReferenceSymbol),
                    node.Accessibility,
                    "$$ctor$$",
                    false
                );

                foreach (VariableNode arg in node.Args)
                {
                    arg.Symbol = new VariableSymbol(
                        ((TypeReferenceNode) Visit(arg.Type)).TypeSymbol,
                        arg.Name
                    );
                    
                    node.DeclarationSymbol.AddChild(arg.Symbol);
                }
                
                break;
            }

            case Pass.VisitTypeMembers:
            {
                _context.Push(node.DeclarationSymbol!);
                VisitChildrenAndParent(node.Children, node.DeclarationSymbol);
                _context.Pop();
                break;
            }
        }
        
        return node;
    }

    public Node LocalVariable(LocalVariableDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public Node MethodCall(MethodCallNode node)
    {
        Symbol? owner = null;
        
        if (node.Target != null)
        {
            Visit(node.Target);

            owner = ResolveIndexableExpressionTarget(node);
        }
        else
        {
            owner = _context.Peek();
        }
        
        bool staticCall = node.Target is TypeReferenceNode;
        if (owner is TypeReferenceSymbol tRef) { owner = tRef.Type; }

        if (owner == null)
        {
            MessageCollection.Error($"Cannot find owner of method {node.MethodName}", node.Location);
        }
        else
        {
            MethodDeclarationSymbol? method = (MethodDeclarationSymbol?) owner.Search(
                x => x is MethodDeclarationSymbol method && method.Name == node.MethodName
            );

            node.DeclarationSymbol = method;

            if (method == null)
            {
                MessageCollection.Error($"Unknown method {node.MethodName}", node.Location);
            }
            else
            {
                // Arg handling
                List<string> signatureList = new();
                foreach (ExpressionNode arg in node.Args)
                {
                    Visit(arg);
                    signatureList.Add(GetExpressionType(arg));
                }
                string signature = String.Join(',', signatureList);

                if (method.Signature != signature)
                {
                    MessageCollection.Error($"Argument mismatch for method {method.Name}:" +
                                            $"\n\tExpected: ({method.Signature})" +
                                            $"\n\tGot:      ({signature})", node.Location);
                }

                // Call semantics
                if (method.IsStatic && !staticCall)
                {
                    // We tried to call the static method by an instance
                    MessageCollection.Error(
                        "Cannot call static method with instance, use the type name instead",
                        node.Location
                    );
                }
                else if (!method.IsStatic && staticCall)
                {
                    // Opposite: we tried to call a non-static method statically
                    MessageCollection.Error(
                        "Cannot call instance method statically",
                        node.Location
                    );
                }
            }
        }

        return node;
    }

    public Node VariableAssignment(VariableAssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public Node Tuple(TupleNode node)
    {
        throw new NotImplementedException();
    }

    public Node Integer(IntegerNode node)
    {
        return node;
    }

    public Node NewClassInstance(NewClassInitializerNode node)
    {
        throw new NotImplementedException();
    }

    public Node CreateArray(ArrayInitializerNode node)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Utility method to get the symbol of a target of an indexable expression.
    /// </summary>
    private Symbol? ResolveIndexableExpressionTarget(IndexableExpressionNode node)
    {
        return node.Target switch
        {
            MemberAccessNode member => member.MemberSymbol,
            MethodCallNode call => throw new NotImplementedException(),
            TypeReferenceNode tRef => tRef.TypeSymbol,
            VariableAssignmentNode asn => throw new NotImplementedException(),
            ArrayInitializerNode array => throw new NotImplementedException(),
            NewClassInitializerNode cls => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Utility method for visiting the list of nodes and setting each child's symbol
    /// as a sub-symbol for the given parent.
    /// </summary>
    private void VisitChildrenAndParent(List<Node> children, Symbol? parent)
    {
        children.ForEach(x =>
        {
            Visit(x);

            // Set parent
            if (parent != null)
            {
                switch (x)
                {
                    case ConstructorDeclarationNode ctor:
                        parent.AddChild(ctor.DeclarationSymbol!);
                        break;
                    case VariableNode var:
                        parent.AddChild(var.Symbol!);
                        break;
                    case MethodDeclarationNode methodDecl:
                        parent.AddChild(methodDecl.DeclarationSymbol!);
                        break;
                    case TypeDefinitionNode typeDef:
                        parent.AddChild(typeDef.DeclarationSymbol!);
                        break;

                    // Can't set parent for expressions
                    case ExpressionNode:
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        });
    }

    /// <summary>
    /// Utility method for getting the type of an expression.
    /// </summary>
    /// <remarks>Always visit the expression before getting its type!</remarks>
    private string GetExpressionType(ExpressionNode expr)
    {
        return expr switch
        {
            IntegerNode => "std::Integer",
            ArrayInitializerNode arr => arr.Type.FullName,
            MemberAccessNode member => GetMemberType(member),
            MethodCallNode call => call.DeclarationSymbol?.Type?.Type?.Name ?? "<???>",
            NewClassInitializerNode cls => cls.Type.FullName,
            TypeReferenceNode tRef => tRef.FullName,
            
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Utility method for getting the type of a member.
    /// </summary>
    /// <remarks>Always visit the expression before getting its type!</remarks>
    private string GetMemberType(MemberAccessNode node)
    {
        return node.MemberSymbol switch
        {
            VariableSymbol var => var.Type?.Type?.Name ?? "<???>",
            MethodDeclarationSymbol method => method.Type?.Type?.Name ?? "<???>",
            TypeReferenceSymbol tRef => tRef.Type?.Name ?? "<???>",
            _ => throw new NotImplementedException()
        };
    }
}