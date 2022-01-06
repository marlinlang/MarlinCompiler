using MarlinCompiler.Ast;
using Ubiquity.NET.Llvm.Values;

namespace MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;

public partial class LlvmCompilationTargetVisitor : IAstVisitor<Value>
{
    public Value Visit(AstNode node)
    {
        return node.Accept(this);
    }

    public Value VisitBlockNode(BlockNode node)
    {
        foreach (AstNode child in node.Children)
        {
            Visit(child);
        }

        return null;
    }

    public Value VisitBooleanNode(BooleanNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitClassDeclarationNode(ClassDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitDoubleNode(DoubleNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitIntegerNode(IntegerNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitMemberAccessNode(MemberAccessNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitMethodDeclarationNode(MethodDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitMethodPrototypeNode(MethodPrototypeNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitMethodCallNode(MethodCallNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitReturnNode(ReturnNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitStringNode(StringNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitVariableAssignmentNode(VariableAssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitVariableDeclarationNode(VariableDeclarationNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitTypeReferenceNode(TypeReferenceNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitNameReferenceNode(NameReferenceNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitArrayInitializerNode(ArrayInitializerNode node)
    {
        throw new NotImplementedException();
    }
}