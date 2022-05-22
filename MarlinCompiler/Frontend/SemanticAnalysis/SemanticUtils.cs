using System.Data;
using System.Runtime.CompilerServices;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Symbols.Kinds;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

public static class SemanticUtils
{
    /// <summary>
    /// Returns the type of an expression.
    /// </summary>
    public static TypeUsageSymbol TypeOfExpr(Analyzer analyzer, ExpressionNode node)
    {
        if (!node.HasMetadata)
        {
            throw new NoNullAllowedException("Node must have metadata");
        }

        if (!node.MetadataIs<SymbolTable>())
        {
            throw new InvalidOperationException("Expressions must have a scope (symbol table) as their metadata.");
        }

        if (node.Location == null)
        {
            throw new NoNullAllowedException("Expression node doesn't have a location");
        }

        SymbolTable scope = node.GetMetadata<SymbolTable>();
        FileLocation location = (FileLocation) node.Location;

        switch (node)
        {
            case VoidTypeReferenceNode:
                return new TypeUsageSymbol(TypeSymbol.Void);

            case IntegerNode:
            {
                return new TypeUsageSymbol(GetTypeOrUnknown(analyzer, "std::Int32", location, scope));
            }

            case TypeReferenceNode typeReferenceNode:
            {
                TypeSymbol typeSymbol = GetTypeOrUnknown(analyzer, typeReferenceNode.FullName, location, scope);

                if (typeSymbol == TypeSymbol.UnknownType)
                {
                    return new TypeUsageSymbol(typeSymbol);
                }

                return typeReferenceNode.GenericTypeArguments.Any()
                           ? AttemptApplyGenerics(analyzer, typeSymbol, typeReferenceNode)
                           : new TypeUsageSymbol(typeSymbol);
            }

            case BinaryOperatorNode binaryOperatorNode:
            case NewClassInitializerNode newClassInitializerNode:
            case InitializerNode initializerNode:
            case MemberAccessNode memberAccessNode:
            case MethodCallNode methodCallNode:
            case VariableAssignmentNode variableAssignmentNode:
            case IndexableExpressionNode indexableExpressionNode:
                throw new NotImplementedException();

            default:
                throw new ArgumentOutOfRangeException(nameof(node));
        }
    }

    /// <summary>
    /// Returns the type of the reference.
    /// </summary>
    public static TypeSymbol TypeOfReference(TypeReferenceNode node)
    {
        if (node.MetadataIs<ISymbol>())
        {
            return node.GetMetadata<ISymbol>() as TypeSymbol ?? throw new NoNullAllowedException();
        }

        if (node.MetadataIs<SymbolTable>())
        {
            return node.GetMetadata<SymbolTable>().PrimarySymbol as TypeSymbol ?? throw new NoNullAllowedException();
        }

        throw new InvalidOperationException("Type reference must have metadata.");
    }

    /// <summary>
    /// Returns whether two types are compatible.
    /// </summary>
    public static bool IsAssignable(Analyzer analyzer, TypeSymbol super, TypeUsageSymbol sub)
    {
        return false;
    }

    /// <summary>
    /// Attempts to apply the generic arguments from <see cref="referenceNode"/> to the <see cref="type"/>
    /// </summary>
    /// <returns>The <see cref="TypeUsageSymbol"/> that was generated alongside semantic checks.</returns>
    private static TypeUsageSymbol AttemptApplyGenerics(Analyzer analyzer, TypeSymbol type, TypeReferenceNode referenceNode)
    {
        if (type == TypeSymbol.UnknownType)
        {
            return new TypeUsageSymbol(type);
        }

        // Only classes have generics
        if (type is not ClassTypeSymbol classType)
        {
            if (referenceNode.GenericTypeArguments.Any())
            {
                analyzer.MessageCollection.Error(
                    $"Cannot pass generic args to non-class type {type.ModuleName}::{type.TypeName}",
                    referenceNode.Location
                );
            }

            return new TypeUsageSymbol(type);
        }

        TypeUsageSymbol result = new(type);
        int paramsCount = classType.GenericParamNames.Length;
        int argsCount = referenceNode.GenericTypeArguments.Length;

        // If the number of arguments is less than the number of parameters, we can't apply generics
        if (argsCount != paramsCount)
        {
            analyzer.MessageCollection.Error(
                $"Generic type arguments do not match the number of generic parameters. Expected {paramsCount}, got {argsCount}.",
                referenceNode.Location
            );

            return result;
        }

        result.GenericArgs = new TypeSymbol[paramsCount];
        for (int i = 0; i < paramsCount; ++i)
        {
            result.GenericArgs[i] = TypeOfReference(referenceNode.GenericTypeArguments[i]);
        }

        return result;
    }

    /// <summary>
    /// Attempts to get a type, otherwise logs an error.
    /// </summary>
    private static TypeSymbol GetTypeOrUnknown(Analyzer analyzer, string name, FileLocation usageLocation, SymbolTable scope)
    {
        ISymbol? symbol = scope.LookupSymbol<SymbolTable>(name).PrimarySymbol;

        if (symbol == null)
        {
            analyzer.MessageCollection.Error($"Unknown type {name}", usageLocation);
            return TypeSymbol.UnknownType;
        }

        if (symbol is not TypeSymbol typeSymbol)
        {
            analyzer.MessageCollection.Error($"{name} is not a type and cannot be used as such", usageLocation);
            return TypeSymbol.UnknownType;
        }

        return typeSymbol;
    }
}