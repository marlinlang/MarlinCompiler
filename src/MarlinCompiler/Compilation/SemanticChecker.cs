using System.Text;
using MarlinCompiler.Antlr;
using MarlinCompiler.Ast;
using MarlinCompiler.Compilation;
using MarlinCompiler.Symbols;

namespace MarlinCompiler.Compilation;

internal class SemanticChecker : BaseAstVisitor<AstNode>
{
    public CompileMessages Messages { get; }
    private readonly Builder _builder;
    
    public SemanticChecker(Builder builder)
    {
        Messages = new CompileMessages();
        _builder = builder;
    }

    /// <summary>
    /// Gets the type of a node as a string.
    /// </summary>
    private string GetNodeTypeName(AstNode node)
    {
        switch (node)
        {
            // Really simple nodes
            case VariableDeclarationNode variableDeclarationNode:
                return variableDeclarationNode?.Type?.Symbol?.Name ?? "<???>";
            case MethodCallNode methodCallNode:
                return ((MethodSymbol) methodCallNode.Symbol)?.Type.Name ?? "<???>";
            case TypeReferenceNode typeReferenceNode:
                return typeReferenceNode.Symbol?.Name ?? "<???>";

            // More complex nodes
            case MemberAccessNode memberAccessNode:
            {
                string type = GetNodeTypeName(memberAccessNode.Member);
                return memberAccessNode.ArrayIndex != null ? type + "[]" : type;
            }
            case NameReferenceNode nameReferenceNode:
                return nameReferenceNode.Symbol switch
                {
                    TypeSymbol ty => ty.Name,
                    MethodSymbol mtd => mtd.Type.Name,
                    VariableSymbol var => var?.Type ?? "<???>",
                    _ => throw new NotImplementedException()
                };

            // Literals
            case BooleanNode:
                return "std::Boolean";
            case IntegerNode:
                return "std::Integer";
            case DoubleNode:
                return "std::Double";
            case StringNode:
                return "std::String";

            default:
                throw new InvalidOperationException("node is not supported");
        }
    }

    /// <summary>
    /// Checks if a type is a subclass of another.
    /// </summary>
    private bool AreTypesCompatible(TypeSymbol super, TypeSymbol sub)
    {
        if (super is ClassTypeSymbol superCls && sub is ClassTypeSymbol subCls)
        {
            return superCls == subCls || subCls.BaseClasses.Contains(super.Name);
        }
        else
        {
            return super == sub;
        }
    }
    
    public override AstNode VisitClassDeclarationNode(ClassDeclarationNode node)
    {
        Symbol[] othersCheck = node.Symbol.LookupMultiple(node.Name);
        if (othersCheck.Length > 1)
        {
            Messages.Error(
                $"Type '{node.Name}' declared multiple times",
                new FileLocation(
                    _builder,
                    ((MarlinParser.ClassDeclarationContext)node.Context).IDENTIFIER().Symbol
                )
            );
        }
        
        VisitChildren(node);

        return node;
    }

    public override AstNode VisitMemberAccessNode(MemberAccessNode node)
    {
        throw new NotImplementedException();
    }

    public override AstNode VisitMethodDeclarationNode(MethodDeclarationNode node)
    {
        if (node.Symbol.Parent is ClassTypeSymbol cls)
        {
            if (cls.IsStatic && !node.IsStatic)
            {
                Messages.Error(
                    $"Cannot have non-static method {node.Name} in static class {cls.Name}",
                    new FileLocation(
                        _builder,
                        ((MarlinParser.MethodDeclarationContext)node.Context).IDENTIFIER().Symbol
                    )
                );
            }
        }

        VisitMethodPrototypeNode(node.Prototype);

        return node;
    }

    public override AstNode VisitMethodPrototypeNode(MethodPrototypeNode node)
    {
        foreach (ArgumentVariableDeclarationNode arg in node.Args)
        {
            if (arg.Type.Symbol == null)
            {
                Messages.Error(
                    $"Unknown type {arg.Type.Name} for argument {arg.Name}",
                    new FileLocation(
                        _builder,
                        ((MarlinParser.ExpectArgContext)arg.Context).IDENTIFIER().Symbol
                    )
                );
            }
        }
        
        VisitChildren(node);
        
        return node;
    }

    public override AstNode VisitMethodCallNode(MethodCallNode node)
    {
        if (node.Symbol == null)
        {
            MarlinParser.MemberAccessContext ctx = ((MarlinParser.MethodCallContext) node.Context).memberAccess();

            StringBuilder argsSb = new("(");
            foreach (ArgumentVariableDeclarationNode arg in node.Args)
            {
                argsSb.Append(arg.Type.Name);
                if (arg != node.Args.Last())
                {
                    argsSb.Append(", ");
                }
            }
            argsSb.Append(')');
            
            // todo: more meaningful error please!
            Messages.Error(
                $"Cannot resolve method {ctx.GetText()}{argsSb}",
                new FileLocation(_builder, ctx.Stop)
            );
        }

        return node;
    }

    public override AstNode VisitVariableAssignmentNode(VariableAssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public override AstNode VisitVariableDeclarationNode(VariableDeclarationNode node)
    {
        if (node.Value != null)
        {
            string valueType = GetNodeTypeName(node.Value);
            if (!AreTypesCompatible((TypeSymbol) node.Type.Symbol, (TypeSymbol) node.Symbol.Lookup(valueType)))
            {
                Messages.Error(
                    $"Cannot assign value of type '{valueType}' to variable '{node.Name}' ('{node.Type.Name}')",
                    new FileLocation(_builder, node.Value.Context.Start)
                );
            }
        }

        return node;
    }
}