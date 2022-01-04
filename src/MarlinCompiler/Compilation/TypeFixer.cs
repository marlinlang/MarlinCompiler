using MarlinCompiler.Ast;
using MarlinCompiler.Compilation;

namespace MarlinCompiler.Symbols;

internal sealed class TypeFixer : BaseAstVisitor<AstNode>
{
    public CompileMessages Messages { get; }
    private Builder _builder;

    public TypeFixer(Builder builder)
    {
        Messages = new CompileMessages();
        _builder = builder;
    }

    /// <summary>
    /// Converts types such as 'int' to std::Integer. Supports array types.
    /// </summary>
    private string FixType(string type)
    {
        // Handle array types
        if (type.EndsWith("[]"))
        {
            return FixType(type[..^2]) + "[]";
        }
        
        return type switch
        {
            "int" => "std::Integer",
            "char" => "std::Character",
            "string" => "std::String",
            "bool" => "std::Boolean",
            "object" => "std::Object",
            _ => type
        };
    }

    public override AstNode VisitBlockNode(BlockNode node)
    {
        if (node is RootBlockNode root && node.Body.Count > 0)
        {
            // Program root
            // Reorder classes
            AstNode[] types = node.Children.Where(x => x is TypeDeclarationNode).ToArray();

            foreach (ClassDeclarationNode classType in types.Where(x => x is ClassDeclarationNode))
            {
                if (classType.BaseClasses.Count == 0 && !classType.IsStatic)
                {
                    classType.BaseClasses.Add(new TypeReferenceNode(null, "std::Object"));
                }
            }
            
            // Reorder classes
            while (true)
            {
                bool foundInaccuracies = false;
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] is ClassDeclarationNode workType)
                    {
                        if (workType.BaseClasses.Count == 0)
                        {
                            continue;
                        }

                        if (!workType.BaseClasses.Any(baseClass =>
                                types.Skip(i).Any(type => ((TypeDeclarationNode) type).Name == baseClass.Name)))
                            continue;

                        foundInaccuracies = true;

                        // Move down
                        // TODO: the base class might not exist and this will crash upon getting to the end of types
                        types[i] = types[i + 1];
                        types[i + 1] = workType;
                    }
                }

                if (!foundInaccuracies)
                {
                    break;
                }
            }

            root.Body.RemoveAll(x => x is TypeDeclarationNode);
            root.Body.InsertRange(0, types);
        }
        
        VisitChildren(node);
        return node;
    }

    public override AstNode VisitClassDeclarationNode(ClassDeclarationNode node)
    {
        string[] baseClasses = new string[node.BaseClasses.Count];
        for (int i = 0; i < node.BaseClasses.Count; i++)
        {
            baseClasses[i] = VisitTypeReferenceNode(node.BaseClasses[i])?.Name;
        }

        node.Symbol = new ClassTypeSymbol(
            node.Name,
            node.Visibility,
            node.IsStatic,
            node.IsSealed,
            baseClasses
        );
        
        foreach (TypeReferenceNode typeRef in node.BaseClasses)
        {
            VisitTypeReferenceNode(typeRef);
        }
        
        VisitChildren(node);

        List<AstNode> children = new();
        foreach (AstNode child in node.Children)
        {
            if (child is VariableDeclarationNode)
            {
                children.Add(child);
            }
        }
        foreach (AstNode child in node.Children)
        {
            if (child is not VariableDeclarationNode)
            {
                children.Add(child);
            }
        }
        node.TypeBody.Body.Clear();
        node.TypeBody.Body.AddRange(children);

        return node;
    }

    public override AstNode VisitMemberAccessNode(MemberAccessNode node)
    {
        if (node.Parent != null) Visit(node.Parent);
        Visit(node.Member);
        return node;
    }

    public override AstNode VisitMethodDeclarationNode(MethodDeclarationNode node)
    {
        List<Symbol> args = new();

        foreach (ArgumentVariableDeclarationNode arg in node.Prototype.Args)
        {
            VisitTypeReferenceNode(arg.Type);
            arg.Symbol = new VariableSymbol(arg.Name, arg.Type.Name);
        }
        
        foreach (AstNode arg in node.Prototype.Args)
        {
            args.Add(arg.Symbol);
        }
        
        node.Symbol = new MethodSymbol(
            node.Name,
            node.IsStatic,
            node.Visibility,
            args
        );
        node.Prototype.Symbol = node.Symbol;
        
        foreach (AstNode arg in node.Prototype.Args)
        {
            node.Symbol.AddChild(arg.Symbol);
        }
        
        Visit(node.Prototype);

        return node;
    }

    public override AstNode VisitMethodPrototypeNode(MethodPrototypeNode node)
    {
        foreach (ArgumentVariableDeclarationNode arg in node.Args)
        {
            Visit(arg);
            VisitTypeReferenceNode(arg.Type);
            arg.Symbol = new VariableSymbol(arg.Name, arg.Type.Name);
        }
        
        VisitChildren(node);

        return node;
    }

    public override AstNode VisitMethodCallNode(MethodCallNode node)
    {
        if (node.Member != null) { Visit(node.Member); }

        foreach (AstNode arg in node.Args)
        {
            Visit(arg);
        }

        return node;
    }

    public override AstNode VisitVariableAssignmentNode(VariableAssignmentNode node)
    {
        Visit(node.Value);
        return node;
    }

    public override AstNode VisitVariableDeclarationNode(VariableDeclarationNode node)
    {
        VisitTypeReferenceNode(node.Type);
        if (node.Value != null) Visit(node.Value);
        
        return node;
    }

    public override AstNode VisitArrayInitializerNode(ArrayInitializerNode node)
    {
        VisitTypeReferenceNode(node.ArrayType);
        return node;
    }

    public override TypeReferenceNode VisitTypeReferenceNode(TypeReferenceNode node)
    {
        node.Name = FixType(node.Name);
        return node;
    }
}