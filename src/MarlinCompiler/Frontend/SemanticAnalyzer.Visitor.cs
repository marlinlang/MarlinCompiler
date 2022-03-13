using System.Data;
using System.Text;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;
using Symbol = MarlinCompiler.Frontend.SemanticAnalyzer.Symbol;

namespace MarlinCompiler.Frontend;

public sealed partial class SemanticAnalyzer : IAstVisitor<Nothing>
{
    public Nothing Visit(Node node)
    {
        node.AcceptVisitor(this);
        
        return null!;
    }

    public Nothing ClassDefinition(ClassTypeDefinitionNode node)
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

    public Nothing ExternedTypeDefinition(ExternedTypeDefinitionNode node)
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

    public Nothing StructDefinition(StructTypeDefinitionNode node)
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

    public Nothing MethodDeclaration(MethodDeclarationNode node)
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

    public Nothing ConstructorDeclaration(ConstructorDeclarationNode node)
    {
        switch (_pass)
        {
            case AnalyzerPass.DefineTypeMembers:
            {
                node.Metadata = new SymbolMetadata(new Symbol(
                    SymbolKind.Constructor,
                    null!,
                    null!,
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

    public Nothing Property(PropertyNode node)
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

    public Nothing LocalVariable(LocalVariableDeclarationNode node)
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

    public Nothing NewClassInstance(NewClassInitializerNode node)
    {
        Symbol? typeSymbol = LookupSymbol(node.Type.FullName, false);

        if (typeSymbol == null)
        {
            MessageCollection.Error($"Unknown type {node.Type.FullName}", node.Type.Location);
        }
        
        // TODO: Look for constructor
        
        return null!;
    }

    public Nothing MethodCall(MethodCallNode node)
    {
        // If we have a Target, that means we're gonna be looking for this method in some type
        // If we don't, we'll just search in our current context
        // Either way, we are going to get a symbol

        Symbol? methodSymbol = null;
        
        if (node.Target != null)
        {
            // TODO
            Visit(node.Target);

            if (node.Target.Metadata == null)
                goto methodFoundCheck;

            ExpressionNode owner = (ExpressionNode) ((SymbolMetadata) node.Target.Metadata).Symbol.Node;
            SemType type = GetExprType(owner);
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

        methodFoundCheck:
        if (methodSymbol == null)
        {
            MessageCollection.Error($"Cannot find method {node.MethodName}", node.Location);
        }
        else
        {
            // Match args to params
            MethodDeclarationNode decl = (MethodDeclarationNode) methodSymbol.Node;

            string[] paramTypes = new string[decl.Parameters.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                SemType type = GetSemType(decl.Parameters[i].Type);
                paramTypes[i] = type.GenericTypeParam == null ? type.Name : $"{type.Name}<>";
            }
            
            string[] argTypes = new string[node.Arguments.Length];
            for (int i = 0; i < argTypes.Length; i++)
            {
                SemType type = GetExprType(node.Arguments[i]);
                argTypes[i] = type.GenericTypeParam == null ? type.Name : $"{type.Name}<>";
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
        }
        
        return null!;
    }

    public Nothing Integer(IntegerNode node) => null!;
}