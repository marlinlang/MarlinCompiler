using System.Data;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Symbols.Kinds;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

public sealed partial class Analyzer
{
    private sealed class MainPass : AstVisitor<None>
    {
        public MainPass(Analyzer analyzer)
        {
            _analyzer     = analyzer;
            _scopeManager = new ScopeManager();
        }

        private readonly Analyzer     _analyzer;
        private readonly ScopeManager _scopeManager;

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
                owner = _scopeManager.CurrentScope;
            }

            // Properties
            if (owner.TryLookupSymbol(node.MemberName, out ISymbol symbol))
            {
                node.SetMetadata(symbol switch
                {
                    // Property
                    PropertySymbol propertySymbol => new TypeUsageSymbol(propertySymbol.Type!.Type),
                    
                    // Method
                    MethodSymbol methodSymbol => new TypeUsageSymbol(methodSymbol.ReturnType!.Type),
                    
                    // Variable/arg
                    VariableSymbol variableSymbol => new TypeUsageSymbol(variableSymbol.Type!.Type),
                    
                    _ => throw new NotImplementedException()
                });
            }
            else
            {
                _analyzer.MessageCollection.Error(
                    $"{(node.Target == null ? "Variable" : "Member")} '{node.MemberName}' not found",
                    node.Location
                );
                node.SetMetadata(new TypeUsageSymbol(TypeSymbol.UnknownType));
            }

            return None.Null;
        }

        public override None ClassDefinition(ClassTypeDefinitionNode node)
        {
            node.BaseType ??= new TypeReferenceNode("std::Object", Array.Empty<TypeReferenceNode>());

            // give the metadata to type ref so it can find the symbol
            node.BaseType.SetMetadata(node.GetMetadata<SymbolTable>());

            // Push scope
            _scopeManager.PushScope(node.GetMetadata<SymbolTable>());

            Visit(node.BaseType);

            foreach (Node member in node)
            {
                Visit(member);
            }

            _scopeManager.PopScope();

            return None.Null;
        }

        public override None ExternTypeDefinition(ExternTypeDefinitionNode node)
        {
            // Push scope
            _scopeManager.PushScope(node.GetMetadata<SymbolTable>());

            foreach (Node member in node)
            {
                Visit(member);
            }

            _scopeManager.PopScope();

            return None.Null;
        }

        public override None StructDefinition(StructTypeDefinitionNode node)
        {
            // Push scope
            _scopeManager.PushScope(node.GetMetadata<SymbolTable>());

            foreach (Node member in node)
            {
                Visit(member);
            }

            _scopeManager.PopScope();

            return None.Null;
        }

        public override None TypeReference(TypeReferenceNode node)
        {
            SemanticUtils.SetTypeRefMetadata(_analyzer, node);

            return None.Null;
        }

        public override None Property(PropertyNode node)
        {
            node.Type.SetMetadata(node.GetMetadata<ISymbol>());
            Visit(node.Type);

            if (node.Value != null)
            {
                Visit(node.Value);

                TypeSymbol varType = SemanticUtils.TypeOfReference(node.Type);
                TypeUsageSymbol typeOfExpr = SemanticUtils.TypeOfExpr(_analyzer, node.Value);
                if (SemanticUtils.IsAssignable(_analyzer, varType, typeOfExpr))
                {
                    _analyzer.MessageCollection.Error("Provided value type doesn't match property type", node.Location);
                }
            }

            return None.Null;
        }

        public override None MethodDeclaration(MethodDeclarationNode node)
        {
            TypeReference(node.Type);

            // Push scope
            _scopeManager.PushScope(node.GetMetadata<SymbolTable>());

            foreach (VariableNode parameter in node.Parameters)
            {
                Visit(parameter);
            }

            foreach (Node statement in node)
            {
                Visit(statement);
            }

            _scopeManager.PopScope();

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
            if (node.Value != null)
            {
                Visit(node.Value);

                TypeSymbol propertyType = SemanticUtils.TypeOfReference(node.Type);
                TypeUsageSymbol typeOfExpr = SemanticUtils.TypeOfExpr(_analyzer, node.Value);
                if (SemanticUtils.IsAssignable(_analyzer, propertyType, typeOfExpr))
                {
                    _analyzer.MessageCollection.Error("Provided value type doesn't match variable type", node.Location);
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
                    throw new NoNullAllowedException("MethodSymbol.ReturnType cannot be null");
                }

                node.SetMetadata(methodSymbol.ReturnType);

                // TODO: Check that the number of arguments matches the method signature

                return None.Null;
            }

            // We couldn't find the method
            node.SetMetadata(new TypeUsageSymbol(TypeSymbol.UnknownType));
            _analyzer.MessageCollection.Error(
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
            throw new NotImplementedException();
        }
    }
}