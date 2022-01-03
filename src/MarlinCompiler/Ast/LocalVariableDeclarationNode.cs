using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public sealed class LocalVariableDeclarationNode : VariableDeclarationNode
{
    public LocalVariableDeclarationNode(ParserRuleContext context, TypeReferenceNode type, string name, AstNode value)
        : base(context, type, name, value, false, MemberVisibility.Private)
    {
    }
}