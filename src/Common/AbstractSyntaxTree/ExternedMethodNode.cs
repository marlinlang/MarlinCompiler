using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Represents a method mapping in an externed type.
/// </summary>
public class ExternedMethodNode : Node
{
    public ExternedMethodNode(
        GetAccessibility accessibility,
        TypeReferenceNode type,
        string? name,
        bool isStatic,
        VariableNode[] parameters,
        ExpressionNode[] passedArgs)
    {
        Accessibility = accessibility;
        Type          = type;
        Name          = name;
        IsStatic      = isStatic;
        Parameters    = parameters;
        PassedArgs    = passedArgs;
    }

    public GetAccessibility  Accessibility { get; }
    public TypeReferenceNode Type          { get; }

    /// <summary>
    /// Null means it's a constructor mapping.
    /// </summary>
    public string? Name { get; }

    public bool             IsStatic   { get; }
    public VariableNode[]   Parameters { get; }
    public ExpressionNode[] PassedArgs { get; }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.ExternedMethodMapping(this);
    }
}