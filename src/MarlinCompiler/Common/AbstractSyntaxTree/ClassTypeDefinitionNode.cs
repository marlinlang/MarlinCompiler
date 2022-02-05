using MarlinCompiler.Common.Semantics.Symbols;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Class declaration.
/// </summary>
public class ClassTypeDefinitionNode : TypeDefinitionNode
{
    /// <summary>
    /// The type that this class inherits from, incl. module path 
    /// </summary>
    /// <remarks>Only null for the std.Object type.</remarks>
    public TypeReferenceNode? BaseType { get; set; }
    
    public bool IsStatic { get; }

    public TypeReferenceSymbol? BaseTypeSymbol { get; set; }
    
    public ClassTypeDefinitionNode(string name, string module, GetAccessibility accessibility,
        bool isStatic, TypeReferenceNode? baseType) : base(name, module, accessibility)
    {
        BaseType = baseType;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.ClassDefinition(this);
    }
}