using System.Data;
using System.Net.Http.Headers;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Semantics;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Intermediate;

public sealed class SemanticAnalyzer : IAstVisitor<Node>
{
    public MessageCollection MessageCollection { get; }

    private Scope _currentScope = null!;

    public SemanticAnalyzer()
    {
        MessageCollection = new MessageCollection();
    }

    public Node Visit(Node node)
    {
        Scope oldScope = _currentScope;

        if (node is ContainerNode container)
        {
            _currentScope = container.Scope ?? oldScope;
        }
        
        Node ret = node.AcceptVisitor(this);
        
        _currentScope = oldScope;

        return ret;
    }

    private void VisitVoid(Node node) => Visit(node);

    public Node ClassDefinition(ClassTypeDefinitionNode node)
    {
        node.Children.ForEach(VisitVoid);

        return node;
    }

    public Node ExternedTypeDefinition(ExternedTypeDefinitionNode node)
    {
        node.Children.ForEach(VisitVoid);

        return node;
    }

    public Node StructDefinition(StructTypeDefinitionNode node)
    {
        node.Children.ForEach(VisitVoid);

        return node;
    }

    public Node Property(PropertyNode node)
    {
        Visit(node.Type);
        
        if (node.Value != null)
        {
            Visit(node.Value);
        }

        return node;
    }

    public Node LocalVariable(LocalVariableDeclarationNode node)
    {
        Visit(node.Type);

        string typeName = GetNodeType(node.Type);
        Symbol? typeSymbol = _currentScope.Lookup(typeName);

        node.Symbol!.AttachedScope = new Scope();

        if (typeSymbol == null)
        {
            MessageCollection.Error($"Unknown type {typeName}", node.Location);
        }
        else
        {
            node.Symbol.AttachedScope.AddFrom(typeSymbol.AttachedScope!);
        }
        
        if (node.Value != null)
        {
            Visit(node.Value);
        }

        return node;
    }

    public Node MethodDeclaration(MethodDeclarationNode node)
    {
        Visit(node.Type);

        foreach (VariableNode arg in node.Args)
        {
            Visit(arg.Type);
            if (node.Args.Count(x => x.Name == arg.Name) > 1)
            {
                MessageCollection.Error($"Repeated argument name {arg.Name}", arg.Location);
            }
        }

        node.Children.ForEach(VisitVoid);
        
        return node;
    }

    public Node VariableAssignment(VariableAssignmentNode node)
    {
        Scope owner = _currentScope;
        bool staticAccess = node.Target is TypeReferenceNode;
        
        if (node.Target != null)
        {
            Visit(node.Target);

            if (node.Target.Symbol == null)
            {
                throw new NoNullAllowedException("Target symbol mustn't be null.");
            }
            
            if (node.Target.Symbol.AttachedScope == null)
            {
                throw new NoNullAllowedException("Attached scope of indexable expression mustn't be null.");
            }
            
            owner = node.Target.Symbol.AttachedScope;
        }

        node.Symbol = owner.Lookup(node.Name);

        if (node.Symbol == null)
        {
            MessageCollection.Error(
                $"Cannot find variable {node.Name}",
                node.Location
            );
        }
        else
        {
            // We have found the variable/property
            Visit(node.Value);
            string valueType = GetNodeType(node.Value);

            if (staticAccess && node.Symbol.Kind is not (SymbolKind.StaticMethod or SymbolKind.StaticProperty))
            {
                MessageCollection.Error(
                    $"Attempted to access non-static value {node.Name} statically, use instance instead",
                    node.Location
                );
            }
            else if (!staticAccess && node.Symbol.Kind is not (SymbolKind.Method or SymbolKind.Variable))
            {
                MessageCollection.Error(
                    "Attempted to access static value non-statically, use type name instead",
                    node.Location
                );
            }
            
            if (node.Symbol.Type != valueType)
            {
                MessageCollection.Error(
                    $"Mismatched types, expected {node.Symbol.Type}, got {valueType}",
                    node.Location
                );
            }
        }

        return node;
    }

    public Node ConstructorDeclaration(ConstructorDeclarationNode node)
    {
        // TODO
        
        return node;
    }

    public Node TypeReference(TypeReferenceNode node)
    {
        node.Symbol = _currentScope.Lookup(node.FullName);
        return node;
    }

    public Node MemberAccess(MemberAccessNode node)
    {
        Scope owner = _currentScope;
        bool staticAccess = node.Target is TypeReferenceNode;
        
        if (node.Target != null)
        {
            Visit(node.Target);

            if (node.Target.Symbol == null)
            {
                throw new NoNullAllowedException("Target symbol mustn't be null.");
            }
            
            if (node.Target.Symbol.AttachedScope == null)
            {
                throw new NoNullAllowedException("Attached scope of indexable expression mustn't be null.");
            }
            
            owner = node.Target.Symbol.AttachedScope;
        }

        node.Symbol = owner.Lookup(node.MemberName);

        if (node.Symbol == null)
        {
            MessageCollection.Error(
                $"Cannot find static variable {node.MemberName}",
                node.Location
            );
        }
        else
        {
            // We have found the member
            if (staticAccess && node.Symbol.Kind is not (SymbolKind.StaticMethod or SymbolKind.StaticProperty))
            {
                MessageCollection.Error(
                    $"Attempted to access non-static value {node.MemberName} statically, use instance instead",
                    node.Location
                );
            }
            else if (!staticAccess && node.Symbol.Kind is not (SymbolKind.Method or SymbolKind.Variable))
            {
                MessageCollection.Error(
                    "Attempted to access static value non-statically, use type name instead",
                    node.Location
                );
            }
        }

        return node;
    }

    public Node Integer(IntegerNode node) => node;

    /// <summary>
    /// Utility method for getting a node's type.
    /// </summary>
    private string GetNodeType(Node node)
    {
        return node switch
        {
            ExternedMethodNode externedMethod => externedMethod.Type.FullName,
            InitializerNode initializer => initializer.Type.FullName,
            MemberAccessNode memberAccess => memberAccess.Symbol?.Type ?? "???",
            MethodCallNode methodCall => methodCall.Symbol?.Type ?? "???",
            TypeReferenceNode typeReference => typeReference.FullName,
            IndexableExpressionNode indexableExpression => indexableExpression.Symbol?.Type ?? "???",
            VariableNode variable => variable.Type.FullName,
            TypeDefinitionNode typeDefinition => $"{typeDefinition.ModuleName}::{typeDefinition.LocalName}",
            
            ConstructorDeclarationNode constructorDeclaration => "std::Void",
            IntegerNode integer => "std::Integer",
            
            BinaryOperatorNode binaryOperator => throw new NotImplementedException(),

            _ => throw new ArgumentOutOfRangeException(nameof(node))
        };
    }
}