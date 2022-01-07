using MarlinCompiler.Ast;
using Ubiquity.NET.Llvm.DebugInfo;
using Ubiquity.NET.Llvm.Interop;
using Ubiquity.NET.Llvm.Types;
using Ubiquity.NET.Llvm.Values;

namespace MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;

public partial class LlvmCompilationTarget : IAstVisitor<Value>
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
        switch (_currentCompilationPhase)
        {
            case Phase.CreateTypes:
            {
                IStructType type = new DebugStructType(_module, node.Name, null, node.Name);
                node.Symbol.CustomTargetData = type;
                break;
            }

            case Phase.CreateMethods:
            case Phase.VisitMethodBodies:
            {
                foreach (MethodDeclarationNode method in node.Children.Where(c => c is MethodDeclarationNode))
                {
                    VisitMethodDeclarationNode(method);
                }

                break;
            }

            case Phase.CreateProperties:
            {
                break;
            }

            case Phase.CreateVirtualTables:
            {
                break;
            }
        }

        return null;
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
        switch (_currentCompilationPhase)
        {
            case Phase.CreateMethods:
            {
                IFunctionType t = _context.GetFunctionType(_context.VoidType);
                IrFunction func = _module.CreateFunction(node.Symbol.GetPath(), t);

                node.Symbol.CustomTargetData = func;
                break;
            }

            case Phase.VisitMethodBodies:
            {
                IrFunction func = (IrFunction) node.Symbol.CustomTargetData;
                BasicBlock block = func.AppendBasicBlock("entry");
                _instructionBuilder.PositionAtEnd(block);
                _instructionBuilder.Return();
                break;
            }
        }

        return null;
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