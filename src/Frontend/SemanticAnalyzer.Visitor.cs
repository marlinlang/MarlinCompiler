using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;

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
                SemType semType = new(
                    name,
                    node.GenericTypeParamName != null
                        ? new SemType(node.GenericTypeParamName, null)
                        : null
                )
                {
                    Scope = PushScope(),
                };
                semType.Scope.DebugName = $"type {name}";
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.ClassType,
                    semType,
                    name,
                    semType.Scope,
                    node
                ));
                
                // Register generic param
                if (node.GenericTypeParamName != null)
                {
                    CurrentScope.AddGenericParam(node.GenericTypeParamName);
                }

                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case AnalyzerPass.DefineTypeMembers:
            case AnalyzerPass.EnterTypeMembers:
            {
                UseScope(((SymbolMetadata) node.Metadata!).Symbol.Scope);
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
                UseScope(((SymbolMetadata) node.Metadata!).Symbol.Scope);
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
                UseScope(((SymbolMetadata) node.Metadata!).Symbol.Scope);
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

                // Check args
                foreach (VariableNode param in node.Parameters)
                {
                    Visit(param.Type);

                    if (param.Name != "_" && node.Parameters.Count(x => x.Name == param.Name) > 1)
                    {
                        MessageCollection.Error($"Repeated parameter name {param.Name}", param.Location);
                    }
                }
                
                // Check body
                UseScope(((SymbolMetadata) node.Metadata!).Symbol.Scope);
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
                    Visit(param.Type);

                    if (param.Name != "_" && node.Parameters.Count(x => x.Name == param.Name) > 1)
                    {
                        MessageCollection.Error($"Repeated parameter name {param.Name}", param.Location);
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
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case AnalyzerPass.EnterTypeMembers:
            {
                Visit(node.Type);

                if (node.Value != null)
                {
                    Visit(node.Value);

                    SymbolMetadata? metadata = node.Value.Metadata as SymbolMetadata;

                    // We should have already errored
                    if (metadata == null)
                    {
                        return null!;
                    }
            
                    throw new NotImplementedException("value-type and var-type check");
                }

                break;
            }
        }

        return null!;
    }

    public None LocalVariable(LocalVariableDeclarationNode node)
    {
        Visit(node.Type);
        
        // Check for variable shadowing, unless variable name is _
        if (node.Name != "_" && CurrentScope.LookupSymbol(node.Name, true) != null)
        {
            MessageCollection.Error($"Variable {node.Name} has already been defined in the same scope", node.Location);
        }
        
        node.Metadata = new SymbolMetadata(new Symbol(
            SymbolKind.Variable,
            ((SymbolMetadata) node.Type.Metadata!).Symbol.Type,
            node.Name,
            _scopes.Peek(),
            node
        ));
        
        AddSymbolToScope((SymbolMetadata) node.Metadata);

        if (node.Value != null)
        {
            Visit(node.Value);

            // We should have already errored
            if (node.Value.Metadata is not SymbolMetadata)
            {
                return null!;
            }
            
            throw new NotImplementedException("value-type and var-type check");
        }

        return null!;
    }

    public None NewClassInstance(NewClassInitializerNode node)
    {
        Visit(node.Type);
        
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
        
        Symbol? methodSymbol;
        
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
                // something went horribly wrong
                return null!;
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
                Visit(decl.Parameters[i].Type);
                paramTypes[i] = GetSemType(decl.Parameters[i].Type).ToString();
            }
            
            string[] argTypes = new string[node.Arguments.Length];
            for (int i = 0; i < argTypes.Length; i++)
            {
                Visit(node.Arguments[i]);

                // We should have already errored
                if (node.Arguments[i].Metadata is not SymbolMetadata metadata)
                {
                    return null!;
                }
                
                argTypes[i] = metadata.Symbol.Type.ToString();
            }
            
            string expected = String.Join(", ", paramTypes);
            string given = String.Join(", ", argTypes);
            
            if (expected != given)
            {
                MessageCollection.Error(
                    $"Mismatched arguments for method {decl.Name}" +
                    $"\n\tExpected: {decl.Name}({expected})" +
                    $"\n\tGiven:    {decl.Name}({given})",
                    node.Location
                );
            }
            
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
                MessageCollection.Error($"Cannot find type {type.Name}", node.Location);
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
            MessageCollection.Error($"Name {node.MemberName} does not exist", node.Location);
        }

        return null!;
    }

    public None TypeReference(TypeReferenceNode node)
    {
        if (node.GenericTypeName != null)
        {
            // Use the generic version of this method for generic types
            
            VisitTypeReferenceGeneric(node);
        }
        else
        {
            // No generic arg
            
            Symbol? type = CurrentScope.LookupType(GetSemType(node));

            if (type != null)
            {
                if (type.Type.GenericTypeParameter != null && node.GenericTypeName == null)
                {
                    // We *require* a generic param/arg but one isn't given!
                    MessageCollection.Error($"Cannot use generic type {type.Name}<{type.Type.GenericTypeParameter}> without generic argument", node.Location);
                }
                node.Metadata = new SymbolMetadata(type);
            }
            else
            {
                // We don't want nulls!
                node.Metadata = new SymbolMetadata(Symbol.UnknownType);
            
                MessageCollection.Error($"Unknown type {node.FullName}", node.Location);
            }
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
        
        Symbol? varSymbol;
        
        if (node.Target != null)
        {
            Visit(node.Target);

            if (node.Target.Metadata == null)
            {
                // We should already have an error for not being able to find the parent
                return null!;
            }

            SemType type = ((SymbolMetadata) node.Target.Metadata).Symbol.Type;
            Scope? typeScope = type.Scope;

            if (typeScope == null)
            {
                // We pooped the pants
                return null!;
            }

            varSymbol = typeScope.LookupSymbol(node.Name, true);
        }
        else
        {
            varSymbol = CurrentScope.LookupSymbol(node.Name, false);
        }

        if (varSymbol == null)
        {
            MessageCollection.Error($"Name {node.Name} does not exist", node.Location);
        }
        else if (varSymbol.Kind is not (SymbolKind.Property or SymbolKind.Variable))
        {
            MessageCollection.Error($"Cannot reference non-variable {varSymbol.Name}", node.Location);
        }
        else if (varSymbol.Type != Symbol.UnknownType.Type)
        {
            Visit(node.Value);

            if (node.Value.Metadata == null)
            {
                // We had an error with the value, we can't proceed
                return null!;
            }

            varSymbol.Type.Scope = varSymbol.Scope;
            SemType varType = varSymbol.Type;
            SemType valueType = ((SymbolMetadata) node.Value.Metadata).Symbol.Type;

            (bool compatible, string expectedFullName, string givenFullName) = AreTypesCompatible(varType, valueType);
            if (!compatible)
            {
                MessageCollection.Error($"Type mismatch for {varSymbol.Kind.ToString().ToLower()} {varSymbol.Name}" +
                                        $"\n\tExpected: {expectedFullName}" +
                                        $"\n\tGiven:    {givenFullName}", node.Location);
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