using System.Data;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Symbols.Kinds;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

/// <summary>
/// Marlin semantic analyzer.
/// </summary>
public class Analyzer : AstVisitor<None>
{
    public Analyzer(IEnumerable<CompilationUnitNode> compilationUnits)
    {
        _compilationUnits = compilationUnits;
        MessageCollection = new MessageCollection();
    }
    
    /// <summary>
    /// Analysis messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    /// <summary>
    /// The compilation units.
    /// </summary>
    private readonly IEnumerable<CompilationUnitNode> _compilationUnits;

    public void Analyze()
    {
        foreach (CompilationUnitNode compilationUnit in _compilationUnits)
        {
            Visit(compilationUnit);
        }
    }

    public override None MemberAccess(MemberAccessNode node)
    {
        throw new NotImplementedException();
    }

    public override None ClassDefinition(ClassTypeDefinitionNode node)
    {
        node.BaseType ??= new TypeReferenceNode("std::Object", Array.Empty<TypeReferenceNode>());

        // give the metadata to type ref so it can find the symbol
        node.BaseType.SetMetadata(node.GetMetadata<SymbolTable>());

        Visit(node.BaseType);

        foreach (Node member in node)
        {
            Visit(member);
        }

        return None.Null;
    }

    public override None ExternTypeDefinition(ExternTypeDefinitionNode node)
    {
        foreach (Node member in node)
        {
            Visit(member);
        }

        return None.Null;
    }

    public override None StructDefinition(StructTypeDefinitionNode node)
    {
        foreach (Node member in node)
        {
            Visit(member);
        }

        return None.Null;
    }

    public override None TypeReference(TypeReferenceNode node)
    {
        if (node is VoidTypeReferenceNode)
        {
            node.SetMetadata(TypeSymbol.Void);
            return None.Null;
        }
        
        if (!node.HasMetadata)
        {
            throw new NoNullAllowedException("TypeReferenceNode must have metadata");
        }

        try
        {
            SymbolTable symbolTable = node.GetMetadata<SymbolTable>();
            SymbolTable symbol = symbolTable.LookupSymbol<SymbolTable>(
                x => x is TypeSymbol typeSymbol && $"{typeSymbol.ModuleName}::{typeSymbol.TypeName}" == node.FullName
            );

            node.SetMetadata(symbol);
        }
        catch (NoNullAllowedException)
        {
            MessageCollection.Error($"Type reference not found: {node.FullName}", node.Location);
        }
        
        return None.Null;
    }

    public override None Property(PropertyNode node)
    {
        node.Type.SetMetadata(node.GetMetadata<ISymbol>());
        Visit(node.Type);

        if (node.Value != null)
        {
            Visit(node.Value);

            TypeSymbol propertyType = SemanticUtils.TypeOfReference(node.Type);
            TypeUsageSymbol typeOfExpr = SemanticUtils.TypeOfExpr(this, node.Value);
            if (SemanticUtils.IsAssignable(this, propertyType, typeOfExpr))
            {
                MessageCollection.Error("Property value type doesn't match property type", node.Location);
            }
        }
        
        return None.Null;
    }

    public override None MethodDeclaration(MethodDeclarationNode node)
    {
        TypeReference(node.Type);
        
        foreach (VariableNode parameter in node.Parameters)
        {
            Visit(parameter);
        }
        
        foreach (Node statement in node)
        {
            Visit(statement);
        }

        return None.Null;
    }

    public override None ExternMethodMapping(ExternMethodNode node)
    {
        throw new NotImplementedException();
    }

    public override None ConstructorDeclaration(ConstructorDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public override None LocalVariable(LocalVariableDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public override None MethodCall(MethodCallNode node)
    {
        throw new NotImplementedException();
    }

    public override None VariableAssignment(VariableAssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public override None Integer(IntegerNode node)
    {
        node.SetMetadata(SemanticUtils.TypeOfExpr(this, node));
        return None.Null;
    }

    public override None NewClassInstance(NewClassInitializerNode node)
    {
        throw new NotImplementedException();
    }
}