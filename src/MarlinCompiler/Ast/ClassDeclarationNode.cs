using Antlr4.Runtime;
using MarlinCompiler.Antlr;

namespace MarlinCompiler.Ast;

public sealed class ClassDeclarationNode : TypeDeclarationNode
{
    public bool IsStatic { get; }
    public bool IsSealed { get; }
    public List<TypeReferenceNode> BaseClasses { get; }

    public ClassDeclarationNode(MarlinParser.ClassDeclarationContext context, string name,
        bool isStatic, bool isSealed, MemberVisibility visibility, List<TypeReferenceNode> baseClasses)
        : base(context, name, visibility)
    {
        IsStatic = isStatic;
        IsSealed = isSealed;
        BaseClasses = baseClasses;
    }

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.VisitClassDeclarationNode(this);
    }
}