using System.Text;
using MarlinCompiler.Antlr;
using MarlinCompiler.Ast;
using MarlinCompiler.Compilation;
using MarlinCompiler.MarlinCompiler.Compilation;
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

    public override AstNode VisitClassDeclarationNode(ClassDeclarationNode node)
    {
        Symbol[] othersCheck = node.Symbol.LookupMultiple(node.Name);
        if (othersCheck.Length > 1)
        {
            Messages.Error(
                $"Type '{node.Name}' declared multiple times",
                new FileLocation(
                    _builder,
                    ((MarlinParser.ClassDeclarationContext) node.Context).IDENTIFIER().Symbol
                )
            );
        }

        VisitChildren(node);

        return node;
    }

    public override AstNode VisitStructDeclarationNode(StructDeclarationNode node)
    {
        Symbol[] othersCheck = node.Symbol.LookupMultiple(node.Name);
        if (othersCheck.Length > 1)
        {
            Messages.Error(
                $"Type '{node.Name}' declared multiple times",
                new FileLocation(
                    _builder,
                    ((MarlinParser.StructDeclarationContext) node.Context).IDENTIFIER().Symbol
                )
            );
        }

        VisitChildren(node);

        return node;
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
            foreach (AstNode arg in node.Args)
            {
                argsSb.Append(SemanticUtils.GetNodeTypeName(arg));
                if (arg != node.Args.Last())
                {
                    argsSb.Append(", ");
                }
            }
            argsSb.Append(')');
            
            // list possible overloads
            List<string> overloads = new();
            Symbol initial = node.Member.Symbol;
            string methodPath = ctx.GetText();
            
            if (initial != null)
            {
                Symbol owner = node.Member.Symbol.Parent;
                methodPath = initial.GetPath();
                foreach (MethodSymbol overload in owner.LookupMultiple(initial.Name).Where(sym => sym is MethodSymbol))
                {
                    StringBuilder args = new(methodPath);
                    args.Append('(');
                    foreach (VariableSymbol arg in overload.Args)
                    {
                        args.Append(arg.Type);

                        if (arg != overload.Args.Last())
                        {
                            args.Append(", ");
                        }
                    }

                    overloads.Add(args.Append(')').ToString());
                }
            }

            if (overloads.Count == 0)
            {
                Messages.Error(
                    $"Cannot resolve method {methodPath}{argsSb}",
                    new FileLocation(_builder, ctx.Stop)
                );
            }
            else
            {
                Messages.Error(
                    $"Cannot find overload {methodPath}{argsSb} - possible overloads are:\n\t"
                        + string.Join("\n\t", overloads),
                    new FileLocation(_builder, ctx.Stop)
                );
            }
        }

        return node;
    }

    public override AstNode VisitVariableAssignmentNode(VariableAssignmentNode node)
    {
        string varName = node.Member.Context.GetText();
        if (node.Member.Symbol == null)
        {
            Messages.Error(
                $"Unknown variable {varName}",
                new FileLocation(_builder, node.Member.Context.Start)
            );
        }
        else
        {
            Symbol symbol = node.Member.Symbol;
            varName = symbol.GetPath();
            
            if (symbol is VariableSymbol varSymbol)
            {
                string valueType = SemanticUtils.GetNodeTypeName(node.Value);
                TypeSymbol super = (TypeSymbol) node.Symbol.Lookup(varSymbol.Type);
                TypeSymbol sub = (TypeSymbol) node.Symbol.Lookup(valueType);
                if (!SemanticUtils.AreTypesCompatible(super, sub))
                {
                    Messages.Error(
                        $"Cannot assign value of type '{valueType}' to variable '{varName}' ('{varSymbol.Type}')",
                        new FileLocation(_builder, node.Value.Context.Start)
                    );
                }
            }
            else
            {
                Messages.Error(
                    $"Cannot assign to non-variable {varName}",
                    new FileLocation(_builder, node.Member.Context.Start)
                );
            }
        }
        
        return node;
    }

    public override AstNode VisitVariableDeclarationNode(VariableDeclarationNode node)
    {
        if (node.Value != null)
        {
            string valueType = SemanticUtils.GetNodeTypeName(node.Value);
            bool isValueArray = valueType.EndsWith("[]");
            if (isValueArray)
            {
                valueType = valueType[..^2];
            }

            if (node.IsNative)
            {
                Messages.Error(
                    "Do not provide values for native variables",
                    new FileLocation(_builder, node.Value.Context.Start)
                );
            }
            else
            {
                TypeSymbol super = (TypeSymbol) node.Type.Symbol;
                TypeSymbol sub = (TypeSymbol) node.Symbol.Lookup(valueType);
                if (!SemanticUtils.AreTypesCompatible(super, sub) || node.Type.IsArray != isValueArray)
                {
                    if (isValueArray) valueType += "[]"; // hack
                    Messages.Error(
                        $"Cannot assign value of type '{valueType}' to variable '{node.Name}' ('{node.Type.Name}')",
                        new FileLocation(_builder, node.Value.Context.Start)
                    );
                }
            }
        }

        return node;
    }
}