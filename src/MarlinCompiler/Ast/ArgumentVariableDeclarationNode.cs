using Antlr4.Runtime;

namespace MarlinCompiler.Ast;

public sealed class ArgumentVariableDeclarationNode : VariableDeclarationNode
{
    public ArgumentVariableDeclarationNode(ParserRuleContext? context, TypeReferenceNode type, string name)
        : base(context, type, name, null, false, MemberVisibility.Private)
    {
    }
}