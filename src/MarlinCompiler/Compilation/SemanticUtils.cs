using MarlinCompiler.Ast;
using MarlinCompiler.Symbols;

namespace MarlinCompiler.MarlinCompiler.Compilation;

public static class SemanticUtils
{
    /// <summary>
    /// Used to signify that a type is native.
    /// </summary>
    public const string NativeTypeIdentifier = "$native$";
    
    /// <summary>
    /// Gets the type of a node as a string.
    /// </summary>
    public static string GetNodeTypeName(AstNode node)
    {
        switch (node)
        {
            // Really simple nodes
            case VariableDeclarationNode variableDeclarationNode:
                return variableDeclarationNode?.Type?.Symbol?.Name ?? "<???>";
            case MethodCallNode methodCallNode:
                if (methodCallNode.IsNative)
                {
                    return "$native$";
                }
                return ((MethodSymbol) methodCallNode.Symbol)?.Type.Name ?? "<???>";
            case TypeReferenceNode typeReferenceNode:
                return typeReferenceNode.Symbol?.Name ?? "<???>";
            case ArrayInitializerNode arrayInitializerNode:
                return GetNodeTypeName(arrayInitializerNode.ArrayType) + "[]";

            // More complex nodes
            case MemberAccessNode memberAccessNode:
            {
                string type = GetNodeTypeName(memberAccessNode.Member);
                return memberAccessNode.ArrayIndex != null ? type + "[]" : type;
            }
            case NameReferenceNode nameReferenceNode:
                return nameReferenceNode.Symbol switch
                {
                    TypeSymbol ty => ty.Name,
                    MethodSymbol mtd => mtd.Type.Name,
                    VariableSymbol var => var?.Type ?? "<???>",
                    _ => throw new NotImplementedException()
                };

            // Literals
            case BooleanNode:
                return "std::Boolean";
            case IntegerNode:
                return "std::Integer";
            case DoubleNode:
                return "std::Double";
            case StringNode:
                return "std::String";
            case CharacterNode:
                return "std::Character";

            default:
                throw new InvalidOperationException($"{node.GetType().Name} nodes aren't supported");
        }
    }
    
    /// <summary>
    /// Checks if a type is a subclass of another.
    /// </summary>
    public static bool AreTypesCompatible(TypeSymbol super, TypeSymbol sub)
    {
        if (super is ClassTypeSymbol superCls && sub is ClassTypeSymbol subCls)
        {
            return superCls == subCls || subCls.BaseClasses.Contains(super.Name);
        }
        else
        {
            return super == sub;
        }
    }

    /// <summary>
    /// Returns whether or not all code paths return something.
    /// </summary>
    public static bool DoAllCodePathsReturn(AstNode root)
    {
        switch (root)
        {
            case ReturnNode n:
                return true;
            
            default:
                foreach (AstNode child in root.Children)
                {
                    if (DoAllCodePathsReturn(child))
                    {
                        return true;
                    }
                }
                return false;
        }
    }
}