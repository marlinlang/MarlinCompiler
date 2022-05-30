using System.Data;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.FileLocations;
using MarlinCompiler.Common.Messages;
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
        // Expressions that don't need metadata lookups
        switch (node)
        {
            case VoidTypeReferenceNode:
                return TypeUsageSymbol.Void;

            case NullNode:
                return TypeUsageSymbol.Null;

            case IntegerNode:
            {
                if (analyzer.CurrentPass is not MainPass mainPassVisitor)
                {
                    // Irrelevant
                    return TypeUsageSymbol.UnknownType;
                }

                return new TypeUsageSymbol(
                    GetTypeOrUnknown("std::Int32", mainPassVisitor.ScopeManager.CurrentScope),
                    false
                );
            }

            case MethodCallNode methodCallNode:
                return methodCallNode.GetMetadata<TypeUsageSymbol>();

            case MemberAccessNode memberAccessNode:
                return memberAccessNode.GetMetadata<TypeUsageSymbol>();

            case NewClassInitializerNode newClassInitializerNode:
                return newClassInitializerNode.GetMetadata<TypeUsageSymbol>();
        }

        // Expressions that do need metadata lookups
        if (!node.HasMetadata)
        {
            throw new NoNullAllowedException("Node must have metadata");
        }

        TypeSymbol? type = null;
        if (!node.MetadataIs<TypeUsageSymbol>())
        {
            if (node is VariableAssignmentNode variableAssignmentNode)
            {
                // This is a variable assignment, so the type of the variable is the type of the expression
                type = variableAssignmentNode.GetMetadata<VariableSymbol>().Type.Type;
            }
            else
            {
                throw new InvalidOperationException("Expressions must have a TypeUsageSymbol as their metadata.");
            }
        }
        else if (type == null)
        {
            type = node.GetMetadata<TypeUsageSymbol>().Type;
        }

        if (type == TypeSymbol.UnknownType)
        {
            return TypeUsageSymbol.UnknownType;
        }

        SymbolTable scope = type.SymbolTable;

        switch (node)
        {
            case TypeReferenceNode typeReferenceNode:
            {
                TypeSymbol typeSymbol = GetTypeOrUnknown(typeReferenceNode.FullName, scope);

                if (typeSymbol == TypeSymbol.UnknownType)
                {
                    return TypeUsageSymbol.UnknownType;
                }

                TypeUsageSymbol sym = typeSymbol is ClassTypeSymbol cls && cls.GenericParamNames.Any()
                                          ? AttemptApplyGenerics(analyzer, typeSymbol, typeReferenceNode)
                                          : new TypeUsageSymbol(typeSymbol, typeReferenceNode.IsNullable);
                sym.TypeReferencedStatically = true;
                return sym;
            }

            case VariableAssignmentNode:
            {
                return node.GetMetadata<VariableSymbol>().Type;
            }

            case BinaryOperatorNode:
                throw new NotImplementedException();

            default:
                throw new ArgumentOutOfRangeException(nameof(node));
        }
    }

    /// <summary>
    /// Returns whether two types are compatible.
    /// </summary>
    public static bool IsAssignable(TypeUsageSymbol super, TypeUsageSymbol sub)
    {
        // 1. Check generic compatibility
        // 2. Check if the types are the same
        // 3. Check if the types are compatible

        // ReSharper disable once ConvertIfStatementToSwitchStatement - bro no?!
        if (super.Type is GenericParamTypeSymbol
            && sub.Type is GenericParamTypeSymbol)
        {
            return super.Type.Name == sub.Type.Name;
        }

        if (sub.Type is GenericParamTypeSymbol
            || super.Type is GenericParamTypeSymbol)
        {
            TypeUsageSymbol genericSymbol = sub.Type is GenericParamTypeSymbol ? sub : super;
            GenericParamTypeSymbol genericParam = (GenericParamTypeSymbol) genericSymbol.Type;

            // Generic arg supplement is in the TypeUsageSymbol of the sub
            // We need to find the index of the generic arg (names are in the defining class)
            // Then we check if the supplement is assignable to super

            ClassTypeSymbol classTypeSymbol = genericParam.Owner;
            for (int i = 0; i < classTypeSymbol.GenericParamNames.Length; ++i)
            {
                if (classTypeSymbol.GenericParamNames[i] == genericParam.Name)
                {
                    if (sub == TypeUsageSymbol.Null)
                    {
                        return super.IsNullable;
                    }
                
                    return genericSymbol == sub
                               ? IsAssignable(super, genericSymbol.GenericArgs[i])
                               : IsAssignable(genericSymbol.GenericArgs[i], sub);
                }
            }
        }

        // Check if types are related (is sub actually a subclass of super)
        if (sub.Type is ClassTypeSymbol subClass
            && super.Type is ClassTypeSymbol superClass)
        {
            // Subclass checking: go up one base class at a time until we hit the superclass or the top of the hierarchy
            ClassTypeSymbol? currentClass = subClass;
            bool found = false;
            while (currentClass != null)
            {
                if (currentClass == superClass)
                {
                    found = true;
                    break;
                }

                currentClass = currentClass.BaseType?.Type as ClassTypeSymbol;
            }

            if (!found)
            {
                return false;
            }
        }
        else if (super.Type is ClassTypeSymbol
                 && sub == TypeUsageSymbol.Null)
        {
            return super.IsNullable;
        }
        else
        {
            // For non-classes, i.e. types that don't have generics and inheritance, just check name matching
            // Cannot assign null to non-class
            if (sub == TypeUsageSymbol.Null)
            {
                return false;
            }

            return super.Type == sub.Type;
        }

        if (super.GenericArgs.Length != sub.GenericArgs.Length)
        {
            // Generic arguments must be the same length
            return false;
        }

        // Check if generic arguments are compatible
        for (int i = 0; i < super.GenericArgs.Length; i++)
        {
            if (!IsAssignable(super.GenericArgs[i], sub.GenericArgs[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Utility method to provide an error for a type usage, if the types aren't assignable.
    /// </summary>
    public static void CheckIncompatibleTypesAndError(
        MessageCollection collection,
        TypeUsageSymbol expected,
        TypeUsageSymbol given,
        FileLocation usageLocation)
    {
        if (expected.Type != TypeSymbol.UnknownType
            && given.Type != TypeSymbol.UnknownType
            && !IsAssignable(expected, given))
        {
            if (given == TypeUsageSymbol.Null)
            {
                collection.Error(
                    MessageId.CannotAssignNullToType,
                    "Cannot assign null to non-null type"
                    + $"\n\tExpected: {expected.GetStringRepresentation()}"
                    + $"\n\tActual:   null",
                    usageLocation
                );
            }
            else
            {
                collection.Error(
                    MessageId.AssignedValueDoesNotMatchType,
                    "Provided value type doesn't match variable type"
                    + $"\n\tExpected: {expected.GetStringRepresentation()}"
                    + $"\n\tActual:   {given.GetStringRepresentation()}",
                    usageLocation
                );
            }
        }
    }

    /// <summary>
    /// Assigns the metadata for a type reference.
    /// </summary>
    public static void SetTypeRefMetadata(Analyzer analyzer, TypeReferenceNode node)
    {
        if (node is VoidTypeReferenceNode)
        {
            node.SetMetadata(TypeUsageSymbol.Void);
            return;
        }

        if (!node.HasMetadata)
        {
            throw new NoNullAllowedException("TypeReferenceNode must have metadata");
        }

        try
        {
            SymbolTable scope = analyzer.CurrentPass.ScopeManager.CurrentScope;

            TypeSymbol typeOrUnknown = GetTypeOrUnknown(node.FullName, scope);

            node.SetMetadata(new TypeUsageSymbol(typeOrUnknown, node.IsNullable));
        }
        catch (NoNullAllowedException)
        {
            analyzer.MessageCollection.Error(MessageId.UnknownType, $"Type reference not found: {node.FullName}", node.Location);
        }
    }

    /// <summary>
    /// Attempts to apply the generic arguments from <see cref="referenceNode"/> to the <see cref="type"/>
    /// </summary>
    /// <returns>The <see cref="TypeUsageSymbol"/> that was generated alongside semantic checks.</returns>
    public static TypeUsageSymbol AttemptApplyGenerics(Analyzer analyzer, TypeSymbol type, TypeReferenceNode referenceNode)
    {
        if (type == TypeSymbol.UnknownType)
        {
            return TypeUsageSymbol.UnknownType;
        }

        // Make sure we have usable types
        foreach (TypeReferenceNode arg in referenceNode.GenericTypeArguments)
        {
            analyzer.CurrentPass.Visitor.Visit(arg);
        }

        // Only classes have generics
        if (type is not ClassTypeSymbol classType)
        {
            if (referenceNode.GenericTypeArguments.Any())
            {
                analyzer.MessageCollection.Error(
                    MessageId.GenericArgsOnNonGenericType,
                    $"Cannot pass generic arguments to non-generic type {type.Name}"
                    + $"\n\tType:       {type.GetStringRepresentation()}"
                    + $"\n\tUsed as:    {referenceNode}",
                    referenceNode.Location
                );
            }

            return new TypeUsageSymbol(type, referenceNode.IsNullable);
        }

        TypeUsageSymbol result = new(type, referenceNode.IsNullable);
        int paramsCount = classType.GenericParamNames.Length;
        int argsCount = referenceNode.GenericTypeArguments.Length;

        // If the number of arguments is less than the number of parameters, we can't apply generics
        if (argsCount != paramsCount)
        {
            analyzer.MessageCollection.Error(
                MessageId.GenericArgsDoNotMatchParams,
                $"Invalid number of generic arguments passed to type {type.Name}"
                + $"\n\tType:       {type.GetStringRepresentation()}"
                + $"\n\tUsed as:    {referenceNode}"
                + $"\n\tExpected generic args:   {paramsCount}"
                + $"\n\tGiven generic args:      {argsCount}",
                referenceNode.Location
            );

            return result;
        }

        // Generic params and args match, so we can apply them
        result.GenericArgs = new TypeUsageSymbol[paramsCount];
        for (int i = 0; i < paramsCount; ++i)
        {
            result.GenericArgs[i] = referenceNode.GenericTypeArguments[i].GetMetadata<TypeUsageSymbol>();
        }

        return result;
    }

    /// <summary>
    /// Returns whether all code paths return a value.
    /// </summary>
    public static bool DoAllCodePathsReturn(Analyzer analyzer, ContainerNode block, TypeUsageSymbol expectedReturnType)
    {
        // If the block contains a return statement, it obviously returns
        // If any sub block returns on all of its sub-paths, the parent block also returns
        // Void is considered to return on all paths

        if (block.FirstOrDefault(x => x is ReturnStatementNode) is ReturnStatementNode found)
        {
            if (expectedReturnType.Type == TypeSymbol.UnknownType)
            {
                return true;
            }

            if (found.Value                != null
                && expectedReturnType.Type != TypeSymbol.Void)
            {
                TypeUsageSymbol actualReturnType = TypeOfExpr(analyzer, found.Value);
                if (!IsAssignable(expectedReturnType, actualReturnType))
                {
                    analyzer.MessageCollection.Error(
                        MessageId.ReturningInvalidType,
                        "Return statement returns incorrect type."
                        + $"\n\tExpected: {expectedReturnType.GetStringRepresentation()}"
                        + $"\n\tActual:   {actualReturnType.GetStringRepresentation()}",
                        found.Value.Location
                    );
                }
            }
            else if (found.Value                != null
                     && expectedReturnType.Type == TypeSymbol.Void)
            {
                analyzer.MessageCollection.Error(
                    MessageId.ReturningValueFromVoidMethod,
                    "Return statement should not be returning a value from a void method.",
                    found.Value.Location
                );
            }
            else if (found.Value                == null
                     && expectedReturnType.Type != TypeSymbol.Void)
            {
                analyzer.MessageCollection.Error(
                    MessageId.ReturningVoidFromNonVoidMethod,
                    "Return statement does not return a value, but the method isn't null.",
                    found.Location
                );
            }

            return true;
        }

        foreach (Node statement in block)
        {
            if (statement is ContainerNode subContainer)
            {
                // If all code paths in this container return a value, then the parent container also returns a value
                if (DoAllCodePathsReturn(analyzer, subContainer, expectedReturnType))
                {
                    return true;
                }
            }
        }

        return expectedReturnType.Type == TypeSymbol.Void;
    }

    /// <summary>
    /// Attempts to get a type, otherwise logs an error.
    /// </summary>
    private static TypeSymbol GetTypeOrUnknown(string name, SymbolTable scope)
    {
        // Find compilation unit (module)
        string[] nameSplit = name.Split("::");
        string moduleName = String.Join("::", nameSplit[..^1]);

        if (moduleName != String.Empty)
        {
            // Find module
            // For generic types, the module is "", this won't be executed
            if (!scope.TryLookupSymbol(moduleName, out ModuleSymbol moduleSymbol))
            {
                return TypeSymbol.UnknownType;
            }

            scope = moduleSymbol.SymbolTable;
        }

        // Find type
        // the Name of types is the mod::typeName, not just the type name
        return !scope.TryLookupSymbol(name, out TypeSymbol type)
                   ? TypeSymbol.UnknownType
                   : type;
    }
}