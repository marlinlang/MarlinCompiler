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
                        node.GenericTypeParamName
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
                // Check args
                foreach (VariableNode param in node.Parameters)
                {
                    Symbol? paramTypeSymbol = LookupSymbol(param.Type.FullName, false);

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
                    Symbol? paramTypeSymbol = LookupSymbol(param.Type.FullName, false);

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
                    null,
                    node
                ));
                AddSymbolToScope((SymbolMetadata) node.Metadata);
                break;
            }

            case AnalyzerPass.EnterTypeMembers:
            {
                Symbol? typeSymbol = LookupSymbol(node.Type.FullName, false);
        
                if (typeSymbol == null)
                {
                    MessageCollection.Error($"Unknown type {node.Type.FullName}", node.Type.Location);
                }
                else if (typeSymbol.Node is not TypeDefinitionNode typeDef)
                {
                    MessageCollection.Error("Property type is not an actual type", node.Type.Location);
                }

                if (node.Value != null)
                {
                    Visit(node.Value);

                    SemType varType = GetSemType(node.Type);
                    SemType valueType = GetExprType(node.Value);
                    if (!DoTypesMatch(varType, valueType))
                    {
                        MessageCollection.Error(
                            $"Mismatched types for property {node.Name}\n\tExpected: {varType.Name}\n\tGiven:    {valueType.Name}",
                            node.Value.Location
                        );
                    }
                }

                break;
            }
        }

        return null!;
    }

    public None LocalVariable(LocalVariableDeclarationNode node)
    {
        Symbol? typeSymbol = LookupSymbol(node.Type.FullName, false);
        
        if (typeSymbol == null)
        {
            MessageCollection.Error($"Unknown type {node.Type.FullName}", node.Type.Location);
        }
        else if (typeSymbol.Node is not TypeDefinitionNode typeDef)
        {
            MessageCollection.Error("Property type is not an actual type", node.Type.Location);
        }
        
        // Check for variable shadowing
        if (LookupSymbol(node.Name, true) != null)
        {
            MessageCollection.Error($"Variable {node.Name} has already been defined in the same scope", node.Location);
        }

        SemType varType = GetSemType(node.Type);
        node.Metadata = new SymbolMetadata(new Symbol(
            SymbolKind.Variable,
            varType,
            node.Name,
            null,
            node
        ));
        AddSymbolToScope((SymbolMetadata) node.Metadata);

        if (node.Value != null)
        {
            Visit(node.Value);

            SemType valueType = GetExprType(node.Value);
            if (!DoTypesMatch(varType, valueType))
            {
                MessageCollection.Error(
                    $"Mismatched types for variable {node.Name}" +
                    $"\n\tExpected: {varType.Name}" +
                    $"\n\tGiven:    {valueType.Name}",
                    node.Value.Location
                );
            }
        }

        return null!;
    }

    public None NewClassInstance(NewClassInitializerNode node)
    {
        Symbol? typeSymbol = LookupSymbol(node.Type.FullName, false);

        if (typeSymbol == null)
        {
            MessageCollection.Error($"Unknown type {node.Type.FullName}", node.Type.Location);
        }
        
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
            Symbol? typeSymbol = LookupSymbol(type.Name, false);

            if (typeSymbol == null)
            {
                throw new NoNullAllowedException("Null targets should not have metadatas set in the nodes");
            }

            methodSymbol = typeSymbol.Scope!.LookupSymbol(node.MethodName, true);
        }
        else
        {
            methodSymbol = LookupSymbol(node.MethodName, false);
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
            // Match args to params
            MethodDeclarationNode decl = (MethodDeclarationNode) methodSymbol.Node;

            string[] paramTypes = new string[decl.Parameters.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                paramTypes[i] = GetSemType(decl.Parameters[i].Type).ToString();
            }
            
            string[] argTypes = new string[node.Arguments.Length];
            for (int i = 0; i < argTypes.Length; i++)
            {
                argTypes[i] = GetExprType(node.Arguments[i]).ToString();
            }

            string parameters = String.Join(", ", paramTypes);
            string arguments = String.Join(", ", argTypes);

            if (parameters != arguments)
            {
                MessageCollection.Error(
                    $"Passed in arguments do not match the signature for method {methodSymbol.Name}:" +
                    $"\n\tSignature:   {methodSymbol.Name}({parameters})" +
                    $"\n\tCalled with: {methodSymbol.Name}({arguments})",
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
        
        Symbol? lookSymbol = null;
        
        if (node.Target != null)
        {
            Visit(node.Target);

            // we had an error with resolving, don't bother further
            if (node.Target.Metadata == null)
            {
                return null!;
            }
            
            SemType type = ((SymbolMetadata) node.Target.Metadata).Symbol.Type;
            lookSymbol = LookupSymbol(type.Name, false);

            if (lookSymbol == null)
            {
                MessageCollection.Error($"Cannot find type {type.Name}");
            }
        }

        Symbol? found;
        
        if (lookSymbol != null)
        {
            // we're looking inside a type
            found = lookSymbol.Scope?.LookupSymbol(node.MemberName, true);
        }
        else
        {
            // we are looking for a type OR for a local var or property etc.
            found = LookupSymbol(node.MemberName, false);
        }

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
        Symbol? type = LookupSymbol(node.FullName, false);

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
            Symbol? typeSymbol = LookupSymbol(type.Name, false);

            if (typeSymbol == null)
            {
                throw new NoNullAllowedException("Null targets should not have metadatas set in the nodes");
            }

            varSymbol = typeSymbol.Scope!.LookupSymbol(node.Name, true);
        }
        else
        {
            varSymbol = LookupSymbol(node.Name, false);
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

            SemType valueType = GetExprType(node.Value);

            if (!DoTypesMatch(varSymbol.Type, valueType))
            {
                MessageCollection.Error(
                    $"Mismatched types for variable/property {node.Name}" +
                    $"\n\tExpected: {varSymbol.Type}" +
                    $"\n\tGiven:    {valueType}",
                    node.Value.Location
                );
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

    public None Integer(IntegerNode node) => null!;
}