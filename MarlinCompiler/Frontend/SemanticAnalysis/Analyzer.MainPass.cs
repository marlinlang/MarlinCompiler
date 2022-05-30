using System.Data;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Messages;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Symbols.Kinds;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

internal sealed class MainPass : AstVisitor<None>, IPass
{
    public MainPass(Analyzer analyzer)
    {
        ScopeManager = new ScopeManager();
        _analyzer    = analyzer;
    }

    public ScopeManager     ScopeManager { get; }
    public AstVisitor<None> Visitor      => this;

    private readonly Analyzer _analyzer;

    public override None MemberAccess(MemberAccessNode node)
    {
        SymbolTable owner;
        TypeUsageSymbol? typeUsageSymbol = null;

        if (node.Target != null)
        {
            Visit(node.Target);
            typeUsageSymbol                          = node.Target.GetMetadata<TypeUsageSymbol>();
            typeUsageSymbol.TypeReferencedStatically = node.Target is TypeReferenceNode;
            if (typeUsageSymbol.IsUnknownType)
            {
                // Type not found
                node.SetMetadata(TypeUsageSymbol.UnknownType);
                return None.Null;
            }

            owner = typeUsageSymbol.Type.SymbolTable;
        }
        else
        {
            owner = ScopeManager.CurrentScope;
        }

        // Properties
        if (owner.TryLookupSymbol(node.MemberName, out ISymbol symbol))
        {
            if (symbol is VariableSymbol { IsInitialized: false } var)
            {
                _analyzer.MessageCollection.Error(
                    MessageId.UninitializedVariableUsage,
                    $"Use of potentially uninitialized variable '{var.Name}'",
                    node.Location
                );
            }

            node.SetMetadata(
                symbol switch
                {
                    // Property
                    PropertySymbol propertySymbol => new TypeUsageSymbol(
                        propertySymbol.Type,
                        typeUsageSymbol?.GenericArgs ?? Array.Empty<TypeUsageSymbol>()
                    ),

                    // Method
                    MethodSymbol methodSymbol => new TypeUsageSymbol(
                        methodSymbol.ReturnType!,
                        typeUsageSymbol?.GenericArgs ?? Array.Empty<TypeUsageSymbol>()
                    ),

                    // Variable/arg
                    VariableSymbol variableSymbol => variableSymbol.Type,

                    _ => throw new InvalidOperationException()
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

            node.SetMetadata(TypeUsageSymbol.UnknownType);
        }

        return None.Null;
    }

    public override None ClassDefinition(ClassTypeDefinitionNode node)
    {
        node.BaseType ??= new TypeReferenceNode("std::Object", false, Array.Empty<TypeReferenceNode>())
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
        if (typeUsageSymbol.IsUnknownType)
        {
            _analyzer.MessageCollection.Error(MessageId.UnknownType, $"Unknown type {node.FullName}", node.Location);
        }
        else
        {
            TypeUsageSymbol usage = SemanticUtils.AttemptApplyGenerics(_analyzer, typeUsageSymbol.Type, node);
            usage.TypeReferencedStatically = true;
            node.SetMetadata(usage);

            if (usage.IsNullable
                && usage.Type is not ClassTypeSymbol)
            {
                // Cannot have nullable types that are not classes
                _analyzer.MessageCollection.Error(
                    MessageId.NullableTypeNotClass,
                    $"Cannot have a nullable type that is not a class",
                    node.Location
                );
            }
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

            SemanticUtils.CheckIncompatibleTypesAndError(
                _analyzer.MessageCollection,
                node.Type.GetMetadata<TypeUsageSymbol>(),
                SemanticUtils.TypeOfExpr(_analyzer, node.Value),
                node.Location
            );
        }

        return None.Null;
    }

    public override None MethodDeclaration(MethodDeclarationNode node)
    {
        Visit(node.Type);

        // Push scope
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

        if (!SemanticUtils.DoAllCodePathsReturn(_analyzer, node, node.Type.GetMetadata<TypeUsageSymbol>()))
        {
            _analyzer.MessageCollection.Error(
                MessageId.NotAllCodePathsReturn,
                "Not all code paths return a value.",
                node.Location
            );
        }

        return None.Null;
    }

    public override None ExternMethodMapping(ExternMethodNode node)
    {
        return None.Null;
    }

    public override None ConstructorDeclaration(ConstructorDeclarationNode node)
    {
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

    public override None LocalVariable(LocalVariableDeclarationNode node)
    {
        node.Type.SetMetadata(ScopeManager.CurrentScope);
        Visit(node.Type);
        node.GetMetadata<VariableSymbol>().Type = node.Type.GetMetadata<TypeUsageSymbol>();

        if (node.Value != null)
        {
            Visit(node.Value);

            SemanticUtils.CheckIncompatibleTypesAndError(
                _analyzer.MessageCollection,
                node.Type.GetMetadata<TypeUsageSymbol>(),
                SemanticUtils.TypeOfExpr(_analyzer, node.Value),
                node.Location
            );
        }

        return None.Null;
    }

    public override None MethodCall(MethodCallNode node)
    {
        // This is the first time we've seen this method call
        // We need to find the method. If we have a target (i.e. this is a method call on a class/object),
        // we need to look in the type of the target. Otherwise, we need to look in the current scope.
        TypeUsageSymbol parent;
        if (node.Target != null)
        {
            Visit(node.Target);
            parent                          = SemanticUtils.TypeOfExpr(_analyzer, node.Target);
            parent.TypeReferencedStatically = node.Target is TypeReferenceNode;
        }
        else
        {
            // Look to nearest type
            SymbolTable typeScope = ScopeManager.CurrentScope.LookupSymbol<TypeSymbol>(x => x is TypeSymbol).SymbolTable;
            parent = new TypeUsageSymbol((TypeSymbol) typeScope.PrimarySymbol!, false);
        }

        if (parent.IsUnknownType)
        {
            // We don't know the type of the target, so we can't resolve the method
            node.SetMetadata(TypeUsageSymbol.UnknownType);

            return None.Null;
        }

        if (parent.IsVoid)
        {
            // We can't call methods on void
            node.SetMetadata(TypeUsageSymbol.UnknownType);

            _analyzer.MessageCollection.Error(
                MessageId.MethodCallOnVoid,
                "Cannot call method on void",
                node.Location
            );

            return None.Null;
        }

        // Now we need to look in the type of the target
        if (parent.Type.SymbolTable.TryLookupSymbol(
                x => x is MethodSymbol testMethodSymbol && testMethodSymbol.Name == node.MethodName,
                out MethodSymbol methodSymbol
            ))
        {
            if (methodSymbol.ReturnType?.IsUnknownType ?? false)
            {
                // We don't know the return type of the method, so we can't resolve the method
                node.SetMetadata(TypeUsageSymbol.UnknownType);

                return None.Null;
            }

            if (methodSymbol.ReturnType == null)
            {
                throw new NoNullAllowedException("MethodSymbol.ReturnType cannot be null.");
            }

            node.SetMetadata(new TypeUsageSymbol(methodSymbol.ReturnType, parent.GenericArgs));

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
                     && node.Target != null // We don't want to warn about static methods called on the same type
                     && !parent.TypeReferencedStatically)
            {
                _analyzer.MessageCollection.Error(
                    MessageId.StaticMethodCallOnInstance,
                    "Cannot call static method on an instance of a non-static type.",
                    node.Location
                );
            }

            if (methodSymbol.Parameters.Length != node.Arguments.Length)
            {
                _analyzer.MessageCollection.Error(
                    MessageId.ArgumentCountMismatch,
                    $"Method {methodSymbol.Name} expects {methodSymbol.Parameters.Length} arguments, but {node.Arguments.Length} were provided.",
                    node.Location
                );
            }
            else
            {
                int args = methodSymbol.Parameters.Length;
                for (int i = 0; i < args; i++)
                {
                    Visit(node.Arguments[i]);
                    TypeUsageSymbol paramType = new(
                        methodSymbol.Parameters[i].Type.GetMetadata<TypeUsageSymbol>(),
                        parent.GenericArgs
                    );
                    TypeUsageSymbol argType = SemanticUtils.TypeOfExpr(_analyzer, node.Arguments[i]);
                    if (!SemanticUtils.IsAssignable(paramType, argType))
                    {
                        if (argType.IsNull)
                        {
                            _analyzer.MessageCollection.Error(
                                MessageId.CannotAssignNullToType,
                                $"For argument {i + 1} to method {parent.GetStringRepresentation()}#{methodSymbol.Name}:"
                                + $"\n\tExpected type: {paramType.GetStringRepresentation()},"
                                + "\n\tBut got:       null",
                                node.Location
                            );   
                        }
                        else
                        {
                            _analyzer.MessageCollection.Error(
                                MessageId.AssignedValueDoesNotMatchType,
                                $"For argument {i + 1} to method {parent.GetStringRepresentation()}#{methodSymbol.Name}:"
                                + $"\n\tExpected type: {paramType.GetStringRepresentation()},"
                                + $"\n\tBut got:       {argType.GetStringRepresentation()}",
                                node.Location
                            );                            
                        }
                    }
                }
            }

            return None.Null;
        }

        // We couldn't find the method
        node.SetMetadata(TypeUsageSymbol.UnknownType);
        _analyzer.MessageCollection.Error(
            MessageId.MemberNotFound,
            $"Cannot find method {node.MethodName} in type {parent.Type.Name}",
            node.Location
        );

        return None.Null;
    }

    public override None VariableAssignment(VariableAssignmentNode node)
    {
        // This is the first time we've seen this method call
        // We need to find the method. If we have a target (i.e. this is a method call on a class/object),
        // we need to look in the type of the target. Otherwise, we need to look in the current scope.
        SymbolTable symbolTable;
        bool isTypeReferencedStatically = false;
        string scopeName = "<local>";

        if (node.Target != null)
        {
            Visit(node.Target);
            TypeUsageSymbol parent = SemanticUtils.TypeOfExpr(_analyzer, node.Target);
            if (parent.IsUnknownType)
            {
                // We don't know the type of the target, so we can't resolve the method
                node.SetMetadata(VariableSymbol.UnknownVariable);

                return None.Null;
            }


            if (parent.IsVoid)
            {
                // We can't call methods on void
                node.SetMetadata(VariableSymbol.UnknownVariable);

                _analyzer.MessageCollection.Error(
                    MessageId.MethodCallOnVoid,
                    "Cannot assign property on void",
                    node.Location
                );

                return None.Null;
            }

            symbolTable                = parent.Type.SymbolTable;
            isTypeReferencedStatically = parent.TypeReferencedStatically;
            scopeName                  = parent.Type.Name;
        }
        else
        {
            // Use this scope
            symbolTable = ScopeManager.CurrentScope;
        }

        // Now we need to look in the type of the target
        if (symbolTable.TryLookupSymbol(
                x => x is VariableSymbol testVarSymbol && testVarSymbol.Name == node.Name,
                out VariableSymbol varSymbol
            ))
        {
            if (varSymbol.Type.IsUnknownType)
            {
                // We don't know the variable type, so we can't resolve the member within it
                node.SetMetadata(VariableSymbol.UnknownVariable);

                return None.Null;
            }

            varSymbol.IsInitialized = true;
            node.SetMetadata(varSymbol);

            if (isTypeReferencedStatically
                && node.Target != null // We don't want to warn about static props called on the same type
                && varSymbol is PropertySymbol { IsStatic: false })
            {
                _analyzer.MessageCollection.Error(
                    MessageId.InstancePropertyAssignmentOnTypeName,
                    "Cannot assign non-static property on a static type reference.",
                    node.Location
                );
            }
            if (varSymbol is PropertySymbol { IsStatic: true }
                && !isTypeReferencedStatically)
            {
                _analyzer.MessageCollection.Error(
                    MessageId.StaticPropertyAssignmentOnInstance,
                    "Cannot assign static property on an instance of a non-static type.",
                    node.Location
                );
            }

            Visit(node.Value);

            SemanticUtils.CheckIncompatibleTypesAndError(
                _analyzer.MessageCollection,
                varSymbol.Type,
                SemanticUtils.TypeOfExpr(_analyzer, node.Value),
                node.Location
            );

            return None.Null;
        }

        // We couldn't find the property/variable
        node.SetMetadata(VariableSymbol.UnknownVariable);
        if (node.Target == null)
        {
            // Variable
            _analyzer.MessageCollection.Error(
                MessageId.VariableNotFound,
                $"Cannot find variable {node.Name}",
                node.Location
            );
        }
        else
        {
            // Property
            _analyzer.MessageCollection.Error(
                MessageId.MemberNotFound,
                $"Cannot find property {node.Name} in type {scopeName}",
                node.Location
            );
        }

        return None.Null;
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

        if (node.Type.GetMetadata<TypeUsageSymbol>().Type is GenericParamTypeSymbol)
        {
            _analyzer.MessageCollection.Error(
                MessageId.GenericParamTypeCannotBeInstantiated,
                $"Cannot instantiate generic parameter {node.Type.GetMetadata<TypeUsageSymbol>().Type.Name}",
                node.Location
            );
        }

        node.SetMetadata(node.Type.GetMetadata<TypeUsageSymbol>());

        return None.Null;
    }

    public override None ReturnStatement(ReturnStatementNode node)
    {
        if (node.Value != null)
        {
            Visit(node.Value);
        }

        return None.Null;
    }

    public override None Null(NullNode node)
    {
        return None.Null;
    }
}