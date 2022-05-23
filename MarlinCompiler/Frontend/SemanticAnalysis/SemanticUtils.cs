using System.Data;
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

        if (!node.MetadataIs<TypeUsageSymbol>())
        {
            throw new InvalidOperationException("Expressions must have a scope (symbol table) as their metadata.");
        }

        TypeSymbol type = node.GetMetadata<TypeUsageSymbol>().Type;
        if (type == TypeSymbol.UnknownType)
        {
            return new TypeUsageSymbol(TypeSymbol.UnknownType);
        }

        SymbolTable scope = type.SymbolTable;
        FileLocation location = node.Location;

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

            case MethodCallNode methodCallNode:
            {
                analyzer.CurrentVisitor.Visit(methodCallNode);
                // we're expecting a TypeUsageSymbol here
                return methodCallNode.GetMetadata<TypeUsageSymbol>();
            }

            case MemberAccessNode memberAccessNode:
            {
                analyzer.CurrentVisitor.Visit(memberAccessNode);
                // we're expecting a TypeUsageSymbol here
                return memberAccessNode.GetMetadata<TypeUsageSymbol>();
            }

            case BinaryOperatorNode binaryOperatorNode:
            case NewClassInitializerNode newClassInitializerNode:
            case InitializerNode initializerNode:
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
    /// Assigns the metadata for a type reference.
    /// </summary>
    public static void SetTypeRefMetadata(Analyzer analyzer, TypeReferenceNode node)
    {
        if (node is VoidTypeReferenceNode)
        {
            node.SetMetadata(TypeSymbol.Void);
            return;
        }

        if (!node.HasMetadata)
        {
            throw new NoNullAllowedException("TypeReferenceNode must have metadata");
        }

        try
        {
            node.SetMetadata(GetTypeOrUnknown(analyzer, node.FullName, node.Location, node.GetMetadata<SymbolTable>()));
        }
        catch (NoNullAllowedException)
        {
            analyzer.MessageCollection.Error($"Type reference not found: {node.FullName}", node.Location);
        }
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
        // Find compilation unit (module)
        string[] nameSplit = name.Split("::");
        string moduleName = String.Join("::", nameSplit[..^1]);

        // Find module
        if (!scope.TryLookupSymbol(moduleName, out ModuleSymbol module))
        {
            return TypeSymbol.UnknownType;
        }

        // Find type
        // the Name of types is the mod::typeName, not just the type name
        return !module.SymbolTable.TryLookupSymbol(name, out TypeSymbol type)
                   ? TypeSymbol.UnknownType
                   : type;
    }
}