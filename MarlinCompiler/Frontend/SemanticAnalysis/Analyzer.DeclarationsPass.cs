using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Symbols.Kinds;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

internal sealed class DeclarationsPass : NoNieVisitor, IPass
{
    public DeclarationsPass(Analyzer analyzer)
    {
        ScopeManager = new ScopeManager();
        
        _analyzer = analyzer;
    }

    public ScopeManager     ScopeManager { get; }
    public AstVisitor<None> Visitor      => this;

    private readonly Analyzer _analyzer;

    public override None MethodDeclaration(MethodDeclarationNode node)
    {
        Visit(node.Type);
        ((MethodSymbol) node.GetMetadata<SymbolTable>().PrimarySymbol!).ReturnType
            = node.Type.GetMetadata<TypeUsageSymbol>();
        
        ScopeManager.PushScope(node.GetMetadata<SymbolTable>());

        foreach (VariableNode parameter in node.Parameters)
        {
            Visit(parameter.Type);
        }

        foreach (Node statement in node)
        {
            Visit(statement);
        }

        ScopeManager.PopScope();

        return None.Null;
    }

    public override None Property(PropertyNode node)
    {
        Visit(node.Type);
        node.GetMetadata<PropertySymbol>().Type = node.Type.GetMetadata<TypeUsageSymbol>();

        return None.Null;
    }

    public override None LocalVariable(LocalVariableDeclarationNode node)
    {
        Visit(node.Type);
        node.GetMetadata<VariableSymbol>().Type = node.Type.GetMetadata<TypeUsageSymbol>();

        return None.Null;
    }

    public override None ClassDefinition(ClassTypeDefinitionNode node)
    {
        ScopeManager.PushScope(node.GetMetadata<SymbolTable>());

        if (node.BaseType != null)
        {
            node.BaseType.SetMetadata(ScopeManager.CurrentScope);
            Visit(node.BaseType);

            ((ClassTypeSymbol) node.GetMetadata<SymbolTable>().PrimarySymbol!).BaseType
                = node.BaseType.GetMetadata<TypeUsageSymbol>();
        }

        foreach (Node member in node)
        {
            Visit(member);
        }
        
        ScopeManager.PopScope();

        return None.Null;
    }

    public override None ExternTypeDefinition(ExternTypeDefinitionNode node)
    {
        ScopeManager.PushScope(node.GetMetadata<SymbolTable>());
        
        foreach (Node member in node)
        {
            Visit(member);
        }
        
        ScopeManager.PopScope();

        return None.Null;
    }

    public override None StructDefinition(StructTypeDefinitionNode node)
    {
        ScopeManager.PushScope(node.GetMetadata<SymbolTable>());
        
        foreach (Node member in node)
        {
            Visit(member);
        }
        
        ScopeManager.PopScope();

        return None.Null;
    }

    public override None TypeReference(TypeReferenceNode node)
    {
        SemanticUtils.SetTypeRefMetadata(_analyzer, node);

        return None.Null;
    }
}