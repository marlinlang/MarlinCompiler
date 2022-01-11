using MarlinCompiler.Antlr;

namespace MarlinCompiler.Ast;

public interface IAstVisitor<TResult>
{
    public TResult Visit(AstNode node);
    public TResult VisitBlockNode(BlockNode node);
    public TResult VisitBooleanNode(BooleanNode node);
    public TResult VisitClassDeclarationNode(ClassDeclarationNode node);
    public TResult VisitStructDeclarationNode(StructDeclarationNode node);
    public TResult VisitDoubleNode(DoubleNode node);
    public TResult VisitIntegerNode(IntegerNode node);
    public TResult VisitMemberAccessNode(MemberAccessNode node);
    public TResult VisitMethodDeclarationNode(MethodDeclarationNode node);
    public TResult VisitMethodPrototypeNode(MethodPrototypeNode node);
    public TResult VisitMethodCallNode(MethodCallNode node);
    public TResult VisitNullNode(NullNode node);
    public TResult VisitReturnNode(ReturnNode node);
    public TResult VisitStringNode(StringNode node);
    public TResult VisitCharacterNode(CharacterNode node);
    public TResult VisitVariableAssignmentNode(VariableAssignmentNode node);
    public TResult VisitVariableDeclarationNode(VariableDeclarationNode node);
    public TResult VisitTypeReferenceNode(TypeReferenceNode node);
    public TResult VisitNameReferenceNode(NameReferenceNode node);
    public TResult VisitArrayInitializerNode(ArrayInitializerNode node);
}