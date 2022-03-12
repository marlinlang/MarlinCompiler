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
        
        if (node.Value != null)
        {
            Visit(node.Value);
        }

        return node;
    }

    public Node MethodDeclaration(MethodDeclarationNode node)
    {
        Visit(node.Type);

        node.Children.ForEach(VisitVoid);
        
        return node;
    }

    public Node VariableAssignment(VariableAssignmentNode node)
    {
        Scope owner = _currentScope;
        bool staticAccess = false;

        string variableType;
        if (node.Target != null)
        {
            Visit(node.Target);

            staticAccess = node.Target is TypeReferenceNode;

            variableType = node.Target?.Symbol?.Type ?? "???";
            if (variableType != "???")
            {
                Symbol? typeSymbol = owner.Lookup(variableType);
                if (typeSymbol != null)
                {
                    owner = typeSymbol.AttachedScope ?? owner;
                }
                else
                {
                    MessageCollection.Error(
                        $"Cannot find type {variableType}",
                        node.Location
                    );
                }
            }
            else
            {
                MessageCollection.Error(
                    $"Cannot find parent of {node.Name}",
                    node.Location
                );
            }
        }
        
        Symbol? var = owner.Lookup(node.Name);

        if (var == null)
        {
            MessageCollection.Error(
                $"Cannot find variable {node.Name}",
                node.Location
            );
        }
        else
        {
            // TODO: Type checking
            variableType = var.Type;
            node.Symbol = new Symbol(node.Name, variableType, SymbolKind.VariableReference) { AccessInstance = var };
        }

        return node;
    }

    public Node ConstructorDeclaration(ConstructorDeclarationNode node)
    {
        node.Children.ForEach(VisitVoid);
        
        return node;
        
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
        bool staticAccess = false;

        if (node.Target != null)
        {
            Visit(node.Target);

            string type = node.Target?.Symbol?.Type ?? "???";
            if (type != "???")
            {
                Symbol? typeSymbol = owner.Lookup(type);
                if (typeSymbol != null)
                {
                    owner = typeSymbol.AttachedScope ?? owner;
                }
                else
                {
                    MessageCollection.Error(
                        $"Cannot find type {type}",
                        node.Location
                    );
                }
            }
        }

        node.Symbol = owner.Lookup(node.MemberName);

        if (node.Symbol == null)
        {
            MessageCollection.Error(
                $"Cannot find variable {node.MemberName}",
                node.Location
            );
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
            TypeReferenceNode typeReference => typeReference.FullName,
            VariableNode variable => variable.Type.FullName,
            TypeDefinitionNode typeDefinition => $"{typeDefinition.ModuleName}::{typeDefinition.LocalName}",
            
            ConstructorDeclarationNode constructorDeclaration => "std::Void",
            IntegerNode integer => "std::Integer",
            
            BinaryOperatorNode binaryOperator => throw new NotImplementedException(),

            _ => throw new ArgumentOutOfRangeException(nameof(node))
        };
    }
}