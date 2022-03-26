using System.Data;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

public sealed partial class SemanticAnalyzer : IAstVisitor<None>
{
    public None Visit(Node node)
    {
        if (node is CompilationUnitNode unit)
        {
            _currentCompilationUnit = unit;
            if (_pass == Pass.DefineTypes)
            {
                CheckDependencies(unit);
            }
        }

        node.AcceptVisitor(this);
        
        if (node is CompilationUnitNode depUnit && _pass == Pass.EnterTypeMembers)
        {
            CheckUnusedDependencies(depUnit);
        }

        return null!;
    }

    public None ClassDefinition(ClassTypeDefinitionNode node)
    {
        switch (_pass)
        {
            case Pass.DefineTypes:
            {
                string name = $"{node.ModuleName}::{node.LocalName}";
                SemType semType = new(
                    name,
                    node.GenericTypeParamName != null
                        ? new SemType(node.GenericTypeParamName, null)
                        : null
                )
                {
                    Scope = PushScope(name),
                };
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

            case Pass.DefineTypeMembers:
            case Pass.EnterTypeMembers:
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
            case Pass.DefineTypes:
            {
                string name = $"{node.ModuleName}::{node.LocalName}";
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.ExternType,
                    new SemType(
                        name,
                        null
                    ),
                    name,
                    PushScope(name),
                    node
                ));
                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case Pass.DefineTypeMembers:
            case Pass.EnterTypeMembers:
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
            case Pass.DefineTypes:
            {
                string name = $"{node.ModuleName}::{node.LocalName}";
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.StructType,
                    new SemType(
                        name,
                        null
                    ),
                    name,
                    PushScope(name),
                    node
                ));
                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case Pass.DefineTypeMembers:
            case Pass.EnterTypeMembers:
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
            case Pass.DefineTypeMembers:
            {
                Visit(node.Type);

                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.Method,
                    ((SymbolMetadata) node.Type.Metadata!).Symbol.Type,
                    node.Name,
                    PushScope(node.Name),
                    node
                )
                {
                    IsStatic = node.IsStatic
                });
                
                if (node.IsStatic && ((SymbolMetadata) node.Type.Metadata!).Symbol.Type.IsGenericParam)
                {
                    MessageCollection.Error(
                        $"Cannot use generic type on static method {node.Name}",
                        node.Location
                    );
                }
                
                // Check args
                foreach (VariableNode param in node.Parameters)
                {
                    Visit(param.Type);

                    Symbol typeSymbol = ((SymbolMetadata) param.Type.Metadata!).Symbol;
                    
                    param.Metadata = new SymbolMetadata(new Symbol(
                        SymbolKind.Variable,
                        typeSymbol.Type,
                        param.Name,
                        CurrentScope,
                        param
                    )
                    {
                        LocalVariableInitialized = true
                    });
                    
                    AddSymbolToScope((SymbolMetadata) param.Metadata);

                    if (typeSymbol == SpecialTypes.Void)
                    {
                        MessageCollection.Error($"Cannot have void as parameter for {param.Name}", param.Type.Location);
                    }

                    if (param.Name != "_" && node.Parameters.Count(x => x.Name == param.Name) > 1)
                    {
                        MessageCollection.Error($"Repeated parameter name {param.Name}", param.Location);
                    }
                }
                
                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case Pass.EnterTypeMembers:
            {

                UseScope(((SymbolMetadata) node.Metadata!).Symbol.Scope);
                
                // Check body
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
            case Pass.DefineTypeMembers:
            {
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.Constructor,
                    null!,
                    "constructor",
                    PushScope("constructor"),
                    node
                )
                {
                    LocalVariableInitialized = true
                });
                
                // Check args
                foreach (VariableNode param in node.Parameters)
                {
                    Visit(param.Type);

                    Symbol typeSymbol = ((SymbolMetadata) param.Type.Metadata!).Symbol;
                    
                    param.Metadata = new SymbolMetadata(new Symbol(
                        SymbolKind.Variable,
                        typeSymbol.Type,
                        param.Name,
                        CurrentScope,
                        param
                    )
                    {
                        LocalVariableInitialized = true
                    });
                    
                    AddSymbolToScope((SymbolMetadata) param.Metadata);

                    if (typeSymbol == SpecialTypes.Void)
                    {
                        MessageCollection.Error($"Cannot have void as parameter for {param.Name}", param.Type.Location);
                    }

                    if (param.Name != "_" && node.Parameters.Count(x => x.Name == param.Name) > 1)
                    {
                        MessageCollection.Error($"Repeated parameter name {param.Name}", param.Location);
                    }
                }
                
                PopScope();
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                
                break;
            }

            case Pass.EnterTypeMembers:
            {
                // First check if we have duplicated constructors
                IEnumerable<string> paramTypes = node.Parameters
                    .Select(param => ((SymbolMetadata) param.Type.Metadata!).Symbol.Type.ToString());
                string paramString = String.Join(", ", paramTypes);

                Symbol constructorSymbol = ((SymbolMetadata) node.Metadata!).Symbol;

                UseScope(constructorSymbol.Scope);
                
                foreach (Symbol sym in constructorSymbol.Scope.LookupMultipleSymbols("constructor", false))
                {
                    if (sym == constructorSymbol) continue;

                    VariableNode[]? symParams = GetParams(sym);
                    if (symParams == null) continue;

                    IEnumerable<string> foreignParamTypes = symParams
                        .Select(param => ((SymbolMetadata) param.Type.Metadata!).Symbol.Type.ToString());
                    string foreignParamString = String.Join(", ", foreignParamTypes);

                    if (paramString == foreignParamString)
                    {
                        MessageCollection.Error("There are multiple constructors with the same parameters", node.Location);
                    }
                }
                
                // Check body
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
            case Pass.DefineTypeMembers:
            {
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.ExternMethod,
                    GetSemType(node.Type),
                    node.Name ?? "constructor",
                    PushScope(node.Name ?? "constructor"),
                    node
                )
                {
                    IsStatic = node.IsStatic
                });
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
            case Pass.DefineTypeMembers:
            {
                Visit(node.Type);

                if (node.IsStatic && ((SymbolMetadata) node.Type.Metadata!).Symbol.Type.IsGenericParam)
                {
                    MessageCollection.Error(
                        $"Cannot use generic type on static property {node.Name}",
                        node.Location
                    );
                }

                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.Property,
                    ((SymbolMetadata) node.Type.Metadata!).Symbol.Type,
                    node.Name,
                    _scopes.Peek(),
                    node
                )
                {
                    IsStatic = node.IsStatic
                });
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case Pass.EnterTypeMembers:
            {
                if (node.Value != null)
                {
                    Visit(node.Value);

                    if (((SymbolMetadata) node.Type.Metadata!).Symbol == SpecialTypes.Void)
                    {
                        MessageCollection.Error($"Cannot have void as type for property {node.Name}",
                            node.Type.Location);
                    }

                    // We should have already errored
                    if (node.Value.Metadata is not SymbolMetadata metadata)
                    {
                        return null!;
                    }

                    Symbol varSymbol = ((SymbolMetadata) node.Metadata!).Symbol;
                    SemType varType = varSymbol.Type;

                    if (varType.IsGenericParam)
                    {
                        MessageCollection.Error(
                            $"Cannot provide an initialization value for generic type {varType} (on property {varSymbol.Name})",
                            node.Location);
                    }
                    else
                    {
                        SemType valueType = metadata.Symbol.Type;

                        (bool compatible, string expectedFullName, string givenFullName) =
                            AreTypesCompatible(varType, valueType);
                        if (!compatible)
                        {
                            MessageCollection.Error($"Type mismatch for variable {varSymbol.Name}" +
                                                    $"\n\tExpected: {expectedFullName}" +
                                                    $"\n\tGiven:    {givenFullName}", node.Location);
                        }
                    }
                }

                break;
            }
        }

        return null!;
    }

    public None LocalVariable(LocalVariableDeclarationNode node)
    {
        Visit(node.Type);

        if (((SymbolMetadata) node.Type.Metadata!).Symbol == SpecialTypes.Void)
        {
            MessageCollection.Error($"Cannot have void as type for variable {node.Name}", node.Type.Location);
        }

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
            if (node.Value.Metadata is not SymbolMetadata metadata)
            {
                return null!;
            }

            Symbol varSymbol = ((SymbolMetadata) node.Metadata!).Symbol;
            SemType varType = varSymbol.Type;
            SemType valueType = metadata.Symbol.Type;

            varSymbol.LocalVariableInitialized = true;

            (bool compatible, string expectedFullName, string givenFullName) = AreTypesCompatible(varType, valueType);
            if (!compatible)
            {
                MessageCollection.Error($"Type mismatch for variable {varSymbol.Name}" +
                                        $"\n\tExpected: {expectedFullName}" +
                                        $"\n\tGiven:    {givenFullName}", node.Location);
            }
        }

        return null!;
    }

    public None NewClassInstance(NewClassInitializerNode node)
    {
        Visit(node.Type);

        Symbol typeSymbol = ((SymbolMetadata) node.Type.Metadata!).Symbol;
        if (typeSymbol == SpecialTypes.Void)
        {
            MessageCollection.Error($"Cannot instantiate void", node.Type.Location);
        }

        node.Metadata = new SymbolMetadata(new Symbol(
            SymbolKind.Instance,
            typeSymbol.Type,
            "$",
            _scopes.Peek(),
            node
        ));
        
        FindOverload("constructor", node.ConstructorArgs, typeSymbol.Scope, true, true, node.Location);

        return null!;
    }

    public None MethodCall(MethodCallNode node)
    {
        // If we have a Target, that means we're gonna be looking for this method in some type
        // If we don't, we'll just search in our current context
        // Either way, we are going to get a symbol

        // Was the method called statically?
        bool invokedStatically = node.Target is TypeReferenceNode or null;

        Scope? lookScope;

        if (node.Target != null)
        {
            Visit(node.Target);

            if (node.Target.Metadata == null)
            {
                // We should already have an error for not being able to find the parent
                return null!;
            }

            SemType type = ((SymbolMetadata) node.Target.Metadata).Symbol.Type;
            lookScope = type.Scope ?? CurrentScope.LookupType(type).Scope;
        }
        else
        {
            lookScope = CurrentScope;
        }


        Symbol? methodSymbol = FindOverload(node.MethodName, node.Arguments, lookScope, false, false, node.Location);

        if (methodSymbol == null) return null!;
        
        node.Metadata = new SymbolMetadata(methodSymbol);

        if (methodSymbol.IsStatic && !invokedStatically)
        {
            // Tried to call by instance
            MessageCollection.Error(
                $"Cannot call static method {methodSymbol.Name} with instance. Use type name instead.",
                node.Location
            );
        }
        else if (invokedStatically && !methodSymbol.IsStatic)
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

        return null!;
    }

    public None MemberAccess(MemberAccessNode node)
    {
        // Was the member referenced statically?
        bool referencedStatically = node.Target is TypeReferenceNode or null;

        Scope useScope = CurrentScope;

        if (node.Target != null)
        {
            Visit(node.Target);

            // we had an error with resolving, don't bother further
            if (node.Target.Metadata == null)
            {
                return null!;
            }

            SemType type = ((SymbolMetadata) node.Target.Metadata).Symbol.Type;

            if (type == SpecialTypes.UnknownType.Type)
            {
                MessageCollection.Error($"Cannot find type {type.Name}", node.Location);
            }

            useScope = type.Scope ?? throw new NoNullAllowedException("type scope");
        }

        Symbol? found = useScope.LookupSymbol(node.MemberName, false);

        // We can only assign a metadata if the symbol isn't null
        if (found != null)
        {
            node.Metadata = new SymbolMetadata(found);

            if (node.Target == null && found.Kind == SymbolKind.Variable && !found.LocalVariableInitialized)
            {
                MessageCollection.Error($"Variable {found.Name} may not be initialized when used. Make sure to initialize it in every code path leading up to this statement.", node.Location);
            }

            if (found.Kind is not (SymbolKind.Property or SymbolKind.Method or SymbolKind.ExternMethod)) return null!;
            
            if (found.IsStatic && !referencedStatically)
            {
                // Tried to call by instance
                MessageCollection.Error(
                    $"Cannot reference static member {found.Name} with instance. Use type name instead.",
                    node.Location
                );
            }
            else if (referencedStatically && !found.IsStatic)
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
        else
        {
            MessageCollection.Error($"Name {node.MemberName} does not exist in {useScope.Name}", node.Location);
        }

        return null!;
    }

    public None TypeReference(TypeReferenceNode node)
    {
        // The symbol of the type
        Symbol type = CurrentScope.LookupType(GetSemType(node));

        // We cannot find the type, error
        if (type == SpecialTypes.UnknownType)
        {
            // We don't want nulls!
            node.Metadata = new SymbolMetadata(SpecialTypes.UnknownType);

            MessageCollection.Error($"Unknown type {node.FullName}", node.Location);
            
            // Check generic arg just to be sure we've dealt with that too
            if (node.GenericTypeName != null)
            {
                Visit(node.GenericTypeName);
            }
            
            return null!;
        }

        // Void
        if (type == SpecialTypes.Void)
        {
            node.Metadata = new SymbolMetadata(SpecialTypes.Void);
            return null!;
        }

        // We have the type!
        // Let's evaluate the generic type as well.

        // Wait! Before that - if this is a generic param itself, it can't have a generic param!!!
        if (type.Kind == SymbolKind.GenericTypeParam && node.GenericTypeName != null)
        {
            MessageCollection.Error(
                $"Cannot use a generic argument with parameter {type.Name}",
                node.Location
            );
        }

        if (type.Type.GenericTypeParameter != null && node.GenericTypeName == null)
        {
            // We *require* a generic param/arg but one isn't given!
            MessageCollection.Error(
                $"Cannot use generic type {type.Name}<{type.Type.GenericTypeParameter}> without generic argument",
                node.Location
            );
        }
        else if (type.Type.GenericTypeParameter == null && node.GenericTypeName != null)
        {
            // We're trying to pass a generic arg to a type that has no generic params
            MessageCollection.Error(
                $"Cannot use regular type {type.Name}<{type.Type.GenericTypeParameter}> with generic argument",
                node.Location
            );
        }
        else if (node.GenericTypeName != null)
        {
            // Let's evaluate the generic param we pass

            Visit(node.GenericTypeName);

            if (((SymbolMetadata) node.GenericTypeName.Metadata!).Symbol == SpecialTypes.Void)
            {
                MessageCollection.Error(
                    $"Cannot use void a generic argument",
                    node.Location
                );
            }
        }

        if (_pass != Pass.DefineTypeMembers)
        {
            // ONLY clone for instance variables - this can only happen during the EnterTypeMembers pass
            // when property values are evaluated and method entered
            Scope newScope = type.Scope.CloneScope();
            type = type with
            {
                Type = type.Type with
                {
                    Scope = newScope
                },
                Scope = newScope
            };
        }

        type.Type.IsGenericParam = type.Kind == SymbolKind.GenericTypeParam;

        type.Type.Scope = type.Scope; // we need to manually override this!

        if (node.GenericTypeName != null)
        {
            // Set generic arg
            Symbol genericType = (node.GenericTypeName.Metadata as SymbolMetadata)!.Symbol;

            type.Type.GenericTypeParameter = genericType.Type;
            ReplaceAllOccurrencesOfType(type.Scope, type.Scope.Generics[0], genericType.Type);
        }

        if (type.Kind != SymbolKind.GenericTypeParam)
        {
            CheckCanUseType(node.FullName, (FileLocation) node.Location!);
        }

        node.Metadata = new SymbolMetadata(type);

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
        else if (varSymbol.Type != SpecialTypes.UnknownType.Type)
        {
            Visit(node.Value);

            if (node.Value.Metadata == null)
            {
                // We had an error with the value, we can't proceed
                return null!;
            }

            SemType varType = varSymbol.Type;
            SemType valueType = ((SymbolMetadata) node.Value.Metadata).Symbol.Type;

            (bool compatible, string expectedFullName, string givenFullName) = AreTypesCompatible(varType, valueType);
            if (!compatible)
            {
                MessageCollection.Error($"Type mismatch for {varSymbol.Kind.ToString().ToLower()} {varSymbol.Name}" +
                                        $"\n\tExpected: {expectedFullName}" +
                                        $"\n\tGiven:    {givenFullName}", node.Location);
            }

            varSymbol.LocalVariableInitialized = true;

            if (varSymbol.Kind == SymbolKind.Property)
            {
                PropertyNode decl = (PropertyNode) varSymbol.Node;

                if (decl.SetAccessibility == SetAccessibility.NoModify)
                {
                    MessageCollection.Error($"Cannot modify get-only property {decl.Name}", node.Location);
                }

                if (varSymbol.IsStatic && !referencedStatically)
                {
                    // Tried to call by instance
                    MessageCollection.Error(
                        $"Cannot reference static property {varSymbol.Name} with instance. Use type name instead.",
                        node.Location
                    );
                }
                else if (referencedStatically && !varSymbol.IsStatic)
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