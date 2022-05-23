using System.Data;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Messages;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Symbols.Kinds;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

internal sealed class MainPass : AstVisitor<None>
{
    public MainPass(Analyzer analyzer)
    {
        _analyzer    = analyzer;
        ScopeManager = new ScopeManager();
    }

    public ScopeManager ScopeManager { get; }

    private readonly Analyzer _analyzer;

    public override None MemberAccess(MemberAccessNode node)
    {
        SymbolTable owner;

        if (node.Target != null)
        {
            Visit(node.Target);
            TypeSymbol type = node.Target.GetMetadata<TypeUsageSymbol>().Type;
            if (type == TypeSymbol.UnknownType)
            {
                // Type not found
                node.SetMetadata(new TypeUsageSymbol(TypeSymbol.UnknownType));
                return None.Null;
            }

            owner = type.SymbolTable;
        }
        else
        {
            owner = ScopeManager.CurrentScope;
        }

        // Properties
        if (owner.TryLookupSymbol(node.MemberName, out ISymbol symbol))
        {
            node.SetMetadata(
                symbol switch
                {
                    // Property
                    PropertySymbol propertySymbol => new TypeUsageSymbol(propertySymbol.Type!.Type),

                    // Method
                    MethodSymbol methodSymbol => new TypeUsageSymbol(methodSymbol.ReturnType!.Type),

                    // Variable/arg
                    VariableSymbol variableSymbol => new TypeUsageSymbol(variableSymbol.Type!.Type),

                    _ => throw new NotImplementedException()
                }
            );
        }
        else
        {
            if (node.Target == null)
            {
                // Variable
                _analyzer.MessageCollection.Error(
                    MessageId.VariableNotFound,
                    $"Variable or member '{node.MemberName}' not found",
                    node.Location
                );
            }
            else
            {
                // Member
                _analyzer.MessageCollection.Error(
                    MessageId.MemberNotFound,
                    $"Member '{node.MemberName}' not found",
                    node.Location
                );
            }

            node.SetMetadata(new TypeUsageSymbol(TypeSymbol.UnknownType));
        }

        return None.Null;
    }

    public override None ClassDefinition(ClassTypeDefinitionNode node)
    {
        node.BaseType ??= new TypeReferenceNode("std::Object", Array.Empty<TypeReferenceNode>())
        {
            Location = node.Location
        };

        // give the metadata to type ref so it can find the symbol
        node.BaseType.SetMetadata(node.GetMetadata<SymbolTable>());

        // Push scope
        ScopeManager.PushScope(node.GetMetadata<SymbolTable>());

        Visit(node.BaseType);

        foreach (Node member in node)
        {
            Visit(member);
        }

        ScopeManager.PopScope();

        return None.Null;
    }

    public override None ExternTypeDefinition(ExternTypeDefinitionNode node)
    {
        // Push scope
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
        // Push scope
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
        node.SetMetadata(ScopeManager.CurrentScope);
        SemanticUtils.SetTypeRefMetadata(_analyzer, node);

        TypeUsageSymbol typeUsageSymbol = node.GetMetadata<TypeUsageSymbol>();
        if (typeUsageSymbol.Type == TypeSymbol.UnknownType)
        {
            _analyzer.MessageCollection.Error(MessageId.UnknownType, $"Unknown type {node.FullName}", node.Location);
        }
        else
        {
            typeUsageSymbol.TypeReferencedStatically = true;
        }

        return None.Null;
    }

    public override None Property(PropertyNode node)
    {
        node.Type.SetMetadata(ScopeManager.CurrentScope);
        Visit(node.Type);

        if (node.Value != null)
        {
            Visit(node.Value);

            TypeSymbol varType = SemanticUtils.TypeOfReference(node.Type);
            TypeUsageSymbol typeOfExpr = SemanticUtils.TypeOfExpr(_analyzer, node.Value);
            if (varType            != TypeSymbol.UnknownType
                && typeOfExpr.Type != TypeSymbol.UnknownType
                && !SemanticUtils.IsAssignable(_analyzer, varType, typeOfExpr))
            {
                _analyzer.MessageCollection.Error(
                    MessageId.AssignedValueDoesNotMatchType,
                    "Provided value type doesn't match variable type"
                    + $"\n\tExpected: {varType.Name}"
                    + $"\n\tActual:   {typeOfExpr.Type.Name}",
                    node.Location
                );
            }
        }

        return None.Null;
    }

    public override None MethodDeclaration(MethodDeclarationNode node)
    {
        TypeReference(node.Type);

        // Push scope
        ScopeManager.PushScope(node.GetMetadata<SymbolTable>());

        foreach (VariableNode parameter in node.Parameters)
        {
            Visit(parameter);
        }

        foreach (Node statement in node)
        {
            Visit(statement);
        }

        ScopeManager.PopScope();

        return None.Null;
    }

    public override None ExternMethodMapping(ExternMethodNode node)
    {
        return None.Null;
    }

    public override None ConstructorDeclaration(ConstructorDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public override None LocalVariable(LocalVariableDeclarationNode node)
    {
        node.Type.SetMetadata(ScopeManager.CurrentScope);
        Visit(node.Type);

        if (node.Value != null)
        {
            Visit(node.Value);

            TypeSymbol varType = SemanticUtils.TypeOfReference(node.Type);
            TypeUsageSymbol typeOfExpr = SemanticUtils.TypeOfExpr(_analyzer, node.Value);
            if (varType            != TypeSymbol.UnknownType
                && typeOfExpr.Type != TypeSymbol.UnknownType
                && !SemanticUtils.IsAssignable(_analyzer, varType, typeOfExpr))
            {
                _analyzer.MessageCollection.Error(
                    MessageId.AssignedValueDoesNotMatchType,
                    "Provided value type doesn't match variable type"
                    + $"\n\tExpected: {varType.Name}"
                    + $"\n\tActual:   {typeOfExpr.Type.Name}",
                    node.Location
                );
            }
        }

        return None.Null;
    }

    public override None MethodCall(MethodCallNode node)
    {
        if (node.HasMetadata)
        {
            // Something already evaluated this method call
            if (!node.MetadataIs<TypeUsageSymbol>())
            {
                throw new InvalidDataException("Method call node metadata must be a TypeUsageSymbol");
            }

            return None.Null;
        }

        // This is the first time we've seen this method call
        // We need to find the method. If we have a target (i.e. this is a method call on a class/object),
        // we need to look in the type of the target. Otherwise, we need to look in the current scope.
        TypeUsageSymbol parent;
        if (node.Target != null)
        {
            Visit(node.Target);
            parent = SemanticUtils.TypeOfExpr(_analyzer, node.Target);
        }
        else
        {
            // Look to nearest type
            SymbolTable currentScope = node.GetMetadata<SymbolTable>();
            SymbolTable typeScope = currentScope.LookupSymbol<SymbolTable>(x => x is TypeSymbol);
            parent = new TypeUsageSymbol((TypeSymbol) typeScope.PrimarySymbol!);
        }

        if (parent.Type == TypeSymbol.UnknownType)
        {
            // We don't know the type of the target, so we can't resolve the method
            node.SetMetadata(new TypeUsageSymbol(TypeSymbol.UnknownType));

            return None.Null;
        }

        // Now we need to look in the type of the target
        if (parent.Type.SymbolTable.TryLookupSymbol(
                x => x is MethodSymbol methodSymbol && methodSymbol.Name == node.MethodName,
                out ISymbol? methodISymbol
            ))
        {
            MethodSymbol methodSymbol = (MethodSymbol) methodISymbol!;

            if (methodSymbol.ReturnType?.Type == TypeSymbol.UnknownType)
            {
                // We don't know the return type of the method, so we can't resolve the method
                node.SetMetadata(new TypeUsageSymbol(TypeSymbol.UnknownType));

                return None.Null;
            }

            if (methodSymbol.ReturnType == null)
            {
                throw new NoNullAllowedException("MethodSymbol.ReturnType cannot be null.");
            }

            node.SetMetadata(methodSymbol.ReturnType);

            if (parent.TypeReferencedStatically
                && !methodSymbol.IsStatic)
            {
                _analyzer.MessageCollection.Error(
                    MessageId.InstanceMethodCallOnTypeName,
                    "Cannot call non-static method on a static type.",
                    node.Location
                );
            }
            else if (methodSymbol.IsStatic
                     && !parent.TypeReferencedStatically)
            {
                _analyzer.MessageCollection.Error(
                    MessageId.StaticMethodCallOnInstance,
                    "Cannot call static method on an instance of a non-static type.",
                    node.Location
                );
            }
            // TODO: Check that the number of arguments matches the method signature

            return None.Null;
        }

        // We couldn't find the method
        node.SetMetadata(new TypeUsageSymbol(TypeSymbol.UnknownType));
        _analyzer.MessageCollection.Error(
            MessageId.MemberNotFound,
            $"Cannot find method {node.MethodName} in type {parent.Type.Name}",
            node.Location
        );

        return None.Null;
    }

    public override None VariableAssignment(VariableAssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public override None Integer(IntegerNode node)
    {
        node.SetMetadata(SemanticUtils.TypeOfExpr(_analyzer, node));
        return None.Null;
    }

    public override None NewClassInstance(NewClassInitializerNode node)
    {
        node.Type.SetMetadata(ScopeManager.CurrentScope);
        Visit(node.Type);

        node.SetMetadata(node.Type.GetMetadata<TypeUsageSymbol>());

        return None.Null;
    }
}