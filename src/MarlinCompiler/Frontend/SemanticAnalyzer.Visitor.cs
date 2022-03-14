using System.Data;
using System.Text;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;
using Symbol = MarlinCompiler.Frontend.SemanticAnalyzer.Symbol;

namespace MarlinCompiler.Frontend;

public sealed partial class SemanticAnalyzer : IAstVisitor<None>
{
    public None Visit(Node node)
    {
        node.AcceptVisitor(this);
        
        return null!;
    }

    public None ClassDefinition(ClassTypeDefinitionNode node)
    {
        switch (_pass)
        {
            case AnalyzerPass.DefineTypes:
            {
                string name = $"{node.ModuleName}::{node.LocalName}";
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.ClassType,
                    new SemType(
                        name,
                        node.GenericTypeParamName != null
                            ? new SemType(node.GenericTypeParamName, null)
                            : null
                    ),
                    name,
                    PushScope(),
                    node
                ));
                
                // Register generic param
                if (node.GenericTypeParamName != null)
                {
                    _scopes.Peek().AddGenericParam(node.GenericTypeParamName);
                }

                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case AnalyzerPass.DefineTypeMembers:
            case AnalyzerPass.EnterTypeMembers:
            {
                UseScope(((SymbolMetadata) node.Metadata!)!.Symbol.Scope!);
                foreach (Node child in node)
                {
                    Visit(child);
                }
                PopScope();
                break;
            }
        }
        
        return null!;
    }

    public None ExternedTypeDefinition(ExternedTypeDefinitionNode node)
    {
        switch (_pass)
        {
            case AnalyzerPass.DefineTypes:
            {
                string name = $"{node.ModuleName}::{node.LocalName}";
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.ExternType,
                    new SemType(
                        name,
                        null
                    ),
                    name,
                    PushScope(),
                    node
                ));
                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case AnalyzerPass.DefineTypeMembers:
            case AnalyzerPass.EnterTypeMembers:
            {
                UseScope(((SymbolMetadata) node.Metadata!)!.Symbol.Scope!);
                foreach (Node child in node)
                {
                    Visit(child);
                }
                PopScope();
                break;
            }
        }
        
        return null!;
    }

    public None StructDefinition(StructTypeDefinitionNode node)
    {
        switch (_pass)
        {
            case AnalyzerPass.DefineTypes:
            {
                string name = $"{node.ModuleName}::{node.LocalName}";
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.StructType,
                    new SemType(
                        name,
                        null
                    ),
                    name,
                    PushScope(),
                    node
                ));
                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case AnalyzerPass.DefineTypeMembers:
            case AnalyzerPass.EnterTypeMembers:
            {
                UseScope(((SymbolMetadata) node.Metadata!)!.Symbol.Scope!);
                foreach (Node child in node)
                {
                    Visit(child);
                }
                PopScope();
                break;
            }
        }
        
        return null!;
    }

    public None MethodDeclaration(MethodDeclarationNode node)
    {
        switch (_pass)
        {
            case AnalyzerPass.DefineTypeMembers:
            {
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.Method,
                    GetSemType(node.Type),
                    node.Name,
                    PushScope(),
                    node
                ));
                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case AnalyzerPass.EnterTypeMembers:
            {
                Visit(node.Type);

                if (node.Type.Metadata != null)
                {
                    Symbol typeSymbol = ((SymbolMetadata) node.Type.Metadata).Symbol;
                    if (typeSymbol.Node is not TypeDefinitionNode typeDef && typeSymbol.Kind != SymbolKind.GenericTypeParam)
                    {
                        MessageCollection.Error("Method return type is not an actual type", node.Type.Location);
                    }
                }

                // Check args
                foreach (VariableNode param in node.Parameters)
                {
                    Symbol? paramTypeSymbol = CurrentScope.LookupType(GetSemType(param.Type));

                    if (paramTypeSymbol == null)
                    {
                        MessageCollection.Error($"Unknown type {param.Type.FullName}", param.Type.Location);
                    }
                    else if (paramTypeSymbol.Node is not TypeDefinitionNode typeDef)
                    {
                        MessageCollection.Error("Parameter type is not an actual type", param.Type.Location);
                    }

                    if (node.Parameters.Count(x => x.Name == param.Name) > 1)
                    {
                        MessageCollection.Error($"Repeated parameter name {param.Name}");
                    }
                }
                
                // Check body
                UseScope(((SymbolMetadata) node.Metadata!)!.Symbol.Scope!);
                foreach (Node child in node)
                {
                    Visit(child);
                }
                PopScope();
                break;
            }
        }

        return null!;
    }

    public None ConstructorDeclaration(ConstructorDeclarationNode node)
    {
        switch (_pass)
        {
            case AnalyzerPass.DefineTypeMembers:
            {
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.Constructor,
                    null!,
                    "constructor",
                    PushScope(),
                    node
                ));
                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case AnalyzerPass.EnterTypeMembers:
            {
                // Check args
                foreach (VariableNode param in node.Parameters)
                {
                    Symbol? paramTypeSymbol = CurrentScope.LookupType(GetSemType(param.Type));

                    if (paramTypeSymbol == null)
                    {
                        MessageCollection.Error($"Unknown type {param.Type.FullName}", param.Type.Location);
                    }
                    else if (paramTypeSymbol.Node is not TypeDefinitionNode typeDef)
                    {
                        MessageCollection.Error("Parameter type is not an actual type", param.Type.Location);
                    }

                    if (node.Parameters.Count(x => x.Name == param.Name) > 1)
                    {
                        MessageCollection.Error($"Repeated parameter name {param.Name}");
                    }
                }
                
                // Check body
                PushScope();
                foreach (Node child in node)
                {
                    Visit(child);
                }
                PopScope();
                break;
            }
        }

        return null!;
    }

    public None ExternedMethodMapping(ExternedMethodNode node)
    {
        switch (_pass)
        {
            case AnalyzerPass.DefineTypeMembers:
            {
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.ExternMethod,
                    GetSemType(node.Type),
                    node.Name ?? "constructor",
                    PushScope(),
                    node
                ));
                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }
        }

        return null!;
    }

    public None Property(PropertyNode node)
    {
        switch (_pass)
        {
            case AnalyzerPass.DefineTypeMembers:
            {
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.Property,
                    GetSemType(node.Type),
                    node.Name,
                    _scopes.Peek(),
                    node
                ));
                ((SymbolMetadata) node.Metadata).Symbol.Type.Scope = CurrentScope;
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case AnalyzerPass.EnterTypeMembers:
            {
                Symbol? typeSymbol = CurrentScope.LookupType(GetSemType(node.Type));
        
                if (typeSymbol == null)
                {
                    MessageCollection.Error($"Unknown type {node.Type.FullName}", node.Type.Location);
                }
                else if (typeSymbol.Node is not TypeDefinitionNode typeDef && typeSymbol.Kind != SymbolKind.GenericTypeParam)
                {
                    MessageCollection.Error("Property type is not an actual type", node.Type.Location);
                }
                else if (typeSymbol.Type.GenericTypeParam != null && node.Type.GenericTypeName == null)
                {
                    // We didn't give a generic type arg
                    MessageCollection.Error($"Missing generic argument for type {typeSymbol.Name}", node.Location);
                }
                else if (typeSymbol.Type.GenericTypeParam == null && node.Type.GenericTypeName != null)
                {
                    // Opposite of above: type isn't generic but we use it as such
                    MessageCollection.Error(
                        $"Type {typeSymbol.Name} cannot be used with a generic argument as it's not a generic type",
                        null//node.Location
                    );
                }
                else if (node.Type.GenericTypeName != null)
                {
                    // Clone type symbol & its scope
                    typeSymbol = typeSymbol with {};
                    typeSymbol.Scope = typeSymbol.Scope.CloneScope();
            
                    // Register the generic arg to the new clone
                    typeSymbol.Scope.SetGenericArgument(0, GetSemType(node.Type.GenericTypeName));
            
                    // By the way: check if that type even exists!!!
                    //if (CurrentScope.LookupType(GetSemType(node.Type.GenericTypeName)) == null)
                    if (CurrentScope.LookupType(GetSemType(node.Type.GenericTypeName)) == null)
                    {
                        //MessageCollection.Error($"Unknown type {node.Type.GenericTypeName.FullName}");
                    }
                }

                if (node.Value != null)
                {
                    Visit(node.Value);

                    SymbolMetadata? metadata = node.Value.Metadata as SymbolMetadata;

                    // We should have already errored
                    if (metadata == null)
                    {
                        return null!;
                    }
            
                    SemType valueType = metadata.Symbol.Type;
                    throw new NotImplementedException("value-type and var-type check");
                }

                break;
            }
        }

        return null!;
    }

    public None LocalVariable(LocalVariableDeclarationNode node)
    {
        Symbol? typeSymbol = CurrentScope.LookupType(GetSemType(node.Type));
        
        if (typeSymbol == null)
        {
            MessageCollection.Error($"Unknown type {node.Type.FullName}", node.Type.Location);
        }
        else if (typeSymbol.Node is not TypeDefinitionNode typeDef)
        {
            MessageCollection.Error("Variable type is not an actual type", node.Type.Location);
        }
        else if (typeSymbol.Type.GenericTypeParam != null && node.Type.GenericTypeName == null)
        {
            // We didn't give a generic type arg
            MessageCollection.Error($"Missing generic argument for type {typeSymbol.Name}", node.Location);
        }
        else if (typeSymbol.Type.GenericTypeParam == null && node.Type.GenericTypeName != null)
        {
            // Opposite of above: type isn't generic but we use it as such
            MessageCollection.Error(
                $"Type {typeSymbol.Name} cannot be used with a generic argument as it's not a generic type",
                null//node.Location
            );
        }
        else if (node.Type.GenericTypeName != null)
        {
            // Clone type symbol & its scope
            typeSymbol = typeSymbol with {};
            typeSymbol.Scope = typeSymbol.Scope.CloneScope();
            
            // Register the generic arg to the new clone
            typeSymbol.Scope.SetGenericArgument(0, GetSemType(node.Type.GenericTypeName));
            
            // By the way: check if that type even exists!!!
            if (CurrentScope.LookupType(GetSemType(node.Type.GenericTypeName)) == null)
            {
                MessageCollection.Error($"Unknown type {node.Type.GenericTypeName.FullName}");
            }
        }
        
        // Check for variable shadowing
        if (CurrentScope.LookupSymbol(node.Name, true) != null)
        {
            MessageCollection.Error($"Variable {node.Name} has already been defined in the same scope", node.Location);
        }

        SemType varType = GetSemType(node.Type);
        
        node.Metadata = new SymbolMetadata(new Symbol(
            SymbolKind.Variable,
            varType,
            node.Name,
            _scopes.Peek(),
            node
        ));
        
        AddSymbolToScope((SymbolMetadata) node.Metadata);

        if (node.Value != null)
        {
            Visit(node.Value);

            SymbolMetadata? metadata = node.Value.Metadata as SymbolMetadata;

            // We should have already errored
            if (metadata == null)
            {
                return null!;
            }
            
            SemType valueType = metadata.Symbol.Type;
            throw new NotImplementedException("value-type and var-type check");
        }

        return null!;
    }

    public None NewClassInstance(NewClassInitializerNode node)
    {
        Symbol? typeSymbol = CurrentScope.LookupType(GetSemType(node.Type));

        if (typeSymbol == null)
        {
            MessageCollection.Error($"Unknown type {node.Type.FullName}", node.Type.Location);
        }
        
        node.Metadata = new SymbolMetadata(new Symbol(
            SymbolKind.Instance,
            GetSemType(node.Type),
            "$",
            _scopes.Peek(),
            node
        ));
        
        // TODO: Look for constructor
        
        return null!;
    }

    public None MethodCall(MethodCallNode node)
    {
        // If we have a Target, that means we're gonna be looking for this method in some type
        // If we don't, we'll just search in our current context
        // Either way, we are going to get a symbol

        // Was the method called statically?
        bool invokedStatically = node.Target is TypeReferenceNode or null;
        
        Symbol? methodSymbol = null;
        
        if (node.Target != null)
        {
            Visit(node.Target);

            if (node.Target.Metadata == null)
            {
                // We should already have an error for not being able to find the parent
                return null!;
            }

            SemType type = ((SymbolMetadata) node.Target.Metadata).Symbol.Type;
            Scope? typeScope = type.Scope ?? CurrentScope.LookupType(type)?.Scope;

            if (typeScope == null)
            {
                throw new NoNullAllowedException("Cannot find type");
            }

            methodSymbol = typeScope.LookupSymbol(node.MethodName, true);
        }
        else
        {
            methodSymbol = CurrentScope.LookupSymbol(node.MethodName, false);
        }

        if (methodSymbol == null)
        {
            MessageCollection.Error($"Cannot find method {node.MethodName}", node.Location);
        }
        else if (methodSymbol.Kind != SymbolKind.Method)
        {
            MessageCollection.Error($"Cannot invoke non-method {methodSymbol.Name}", node.Location);
        }
        else
        {
            node.Metadata = new SymbolMetadata(methodSymbol);
            
            // Match args to params
            MethodDeclarationNode decl = (MethodDeclarationNode) methodSymbol.Node;

            string[] paramTypes = new string[decl.Parameters.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                Visit(decl.Parameters[i]);
                paramTypes[i] = GetSemType(decl.Parameters[i].Type).ToString();
            }
            
            string[] argTypes = new string[node.Arguments.Length];
            for (int i = 0; i < argTypes.Length; i++)
            {
                Visit(node.Arguments[i]);

                SymbolMetadata? metadata = node.Arguments[i].Metadata as SymbolMetadata;

                // We should have already errored
                if (metadata == null)
                {
                    return null!;
                }
                
                argTypes[i] = metadata.Symbol.ToString();
            }

            string parameters = String.Join(", ", paramTypes);
            string arguments = String.Join(", ", argTypes);

            // TODO: Generics proper support, using <> isn't enough!!!
            throw new NotImplementedException();

            if (decl.IsStatic && !invokedStatically)
            {
                // Tried to call by instance
                MessageCollection.Error(
                    $"Cannot call static method {methodSymbol.Name} with instance. Use type name instead.",
                    node.Location
                );
            }
            else if (invokedStatically && !decl.IsStatic)
            {
                // Tried to call a static method without reference
                // We must double-check that we used a type reference
                if (node.Target != null)
                {
                    MessageCollection.Error(
                        $"Cannot call instance method {methodSymbol.Name} statically. Use an instance instead.",
                        node.Location
                    );
                }
            }
        }
        
        return null!;
    }

    public None MemberAccess(MemberAccessNode node)
    {
        // Was the member referenced statically?
        bool referencedStatically = node.Target is TypeReferenceNode or null;
        
        Scope? lookScope = CurrentScope;
        
        if (node.Target != null)
        {
            Visit(node.Target);

            // we had an error with resolving, don't bother further
            if (node.Target.Metadata == null)
            {
                return null!;
            }
            
            SemType type = ((SymbolMetadata) node.Target.Metadata).Symbol.Type;
            Scope? typeScope = type.Scope ?? CurrentScope.LookupType(type)?.Scope;

            if (typeScope == null)
            {
                MessageCollection.Error($"Cannot find type {type.Name}");
            }
        }

        Symbol? found = CurrentScope.LookupSymbol(node.MemberName, false);
        
        // We can only assign a metadata if the symbol isn't null
        if (found != null)
        {
            node.Metadata = new SymbolMetadata(found);
            
            if (found.Kind is SymbolKind.Property or SymbolKind.Method or SymbolKind.ExternMethod)
            {
                bool isDeclStatic = found.Kind switch
                {
                    SymbolKind.Property => ((PropertyNode) found.Node).IsStatic,
                    SymbolKind.Method => ((MethodDeclarationNode) found.Node).IsStatic,
                    SymbolKind.ExternMethod => ((ExternedMethodNode) found.Node).IsStatic,
                    
                    // C# thinks this is possible, not me ¯\_(ツ)_/¯
                    _ => throw new InvalidOperationException()
                };
                
                if (isDeclStatic && !referencedStatically)
                {
                    // Tried to call by instance
                    MessageCollection.Error(
                        $"Cannot reference static member {found.Name} with instance. Use type name instead.",
                        node.Location
                    );
                }
                else if (referencedStatically && !isDeclStatic)
                {
                    // Tried to reference a static property without reference
                    // We must double-check that we used a type reference
                    if (node.Target != null)
                    {
                        MessageCollection.Error(
                            $"Cannot reference instance property {found.Name} statically. Use an instance instead.",
                            node.Location
                        );
                    }
                }
            }
        }
        else
        {
            MessageCollection.Error($"Cannot find member {node.MemberName}", node.Location);
        }

        return null!;
    }

    public None TypeReference(TypeReferenceNode node)
    {
        Symbol? type = CurrentScope.LookupType(GetSemType(node));

        if (type != null)
        {
            node.Metadata = new SymbolMetadata(type);
        }
        else
        {
            MessageCollection.Error($"Unknown type {node.FullName}", node.Location);
        }

        return null!;
    }

    public None VariableAssignment(VariableAssignmentNode node)
    {
        // If we have a Target, that means we're gonna be looking for this method in some type
        // If we don't, we'll just search in our current context
        // Either way, we are going to get a symbol

        // Was the variable referenced statically?
        bool referencedStatically = node.Target is TypeReferenceNode or null;
        
        Symbol? varSymbol = null;
        
        if (node.Target != null)
        {
            Visit(node.Target);

            if (node.Target.Metadata == null)
            {
                // We should already have an error for not being able to find the parent
                return null!;
            }

            SemType type = ((SymbolMetadata) node.Target.Metadata).Symbol.Type;
            Scope? typeScope = type.Scope ?? CurrentScope.LookupType(type)?.Scope;

            if (typeScope == null)
            {
                throw new NoNullAllowedException("Null targets should not have metadatas set in the nodes");
            }

            varSymbol = typeScope.LookupSymbol(node.Name, true);
        }
        else
        {
            varSymbol = CurrentScope.LookupSymbol(node.Name, false);
        }

        if (varSymbol == null)
        {
            MessageCollection.Error($"Cannot find variable/property {node.Name}", node.Location);
        }
        else if (varSymbol.Kind is not (SymbolKind.Property or SymbolKind.Variable))
        {
            MessageCollection.Error($"Cannot reference non-variable {varSymbol.Name}", node.Location);
        }
        else
        {
            Visit(node.Value);

            if (node.Value.Metadata == null)
            {
                // We had an error with the value, we can't proceed
                return null!;
            }

            SemType varType = varSymbol.Type with {};
            SemType valueType = ((SymbolMetadata) node.Value.Metadata).Symbol.Type;
            
            if (!AreTypesCompatible(ref varType, valueType))
            {
                MessageCollection.Error($"Type mismatch for variable/property {varSymbol.Name}" +
                                        $"\n\tExpected: {varType}" +
                                        $"\n\tGiven:    {valueType}", node.Location);
            }
            
            if (varSymbol.Kind == SymbolKind.Property)
            {
                PropertyNode decl = (PropertyNode) varSymbol.Node;

                if (decl.SetAccessibility == SetAccessibility.NoModify)
                {
                    MessageCollection.Error($"Cannot modify get-only property {decl.Name}", node.Location);
                }
                
                if (decl.IsStatic && !referencedStatically)
                {
                    // Tried to call by instance
                    MessageCollection.Error(
                        $"Cannot reference static property {varSymbol.Name} with instance. Use type name instead.",
                        node.Location
                    );
                }
                else if (referencedStatically && !decl.IsStatic)
                {
                    // Tried to reference a static property without reference
                    // We must double-check that we used a type reference
                    if (node.Target != null)
                    {
                        MessageCollection.Error(
                            $"Cannot reference instance property {varSymbol.Name} statically. Use an instance instead.",
                            node.Location
                        );
                    }
                }
            }
        }

        return null!;
    }

    public None Integer(IntegerNode node)
    {
        node.Metadata = new SymbolMetadata(new Symbol(
            SymbolKind.Instance,
            new SemType("std::Int32", null),
            "$",
            _scopes.Peek(),
            node
        ));
        
        return null!;
    }
}