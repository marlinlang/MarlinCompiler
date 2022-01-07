using MarlinCompiler.Ast;
using MarlinCompiler.Symbols;
using Ubiquity.NET.Llvm.DebugInfo;
using Ubiquity.NET.Llvm.Instructions;
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
        switch (_currentGenerationPhase)
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

            case Phase.FinalizeTypes:
            {
                IStructType type = (IStructType) node.Symbol.CustomTargetData;

                AstNode[] properties = node.Children.Where(x => x is VariableDeclarationNode).ToArray();
                ITypeRef[] elements = new ITypeRef[properties.Count() + 1];

                int i = 0;
                elements[i++] = _context.VoidType;
                foreach (VariableDeclarationNode property in properties)
                {
                    elements[i++] = property.IsNative
                        ? GetNativeTypeRef(property.Type)
                        : GetTypeRef(property.Type);
                }
                
                type.SetBody(false, elements);
                
                GenerateDefaultConstructor(type, node);
                break;
            }
        }

        return null;
    }

    public Value VisitStructDeclarationNode(StructDeclarationNode node)
    {
        switch (_currentGenerationPhase)
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

            case Phase.FinalizeTypes:
            {
                IStructType type = (IStructType) node.Symbol.CustomTargetData;

                AstNode[] properties = node.Children.Where(x => x is VariableDeclarationNode).ToArray();
                ITypeRef[] elements = new ITypeRef[properties.Count()];

                for (var i = 0; i < properties.Length; i++)
                {
                    VariableDeclarationNode property = (VariableDeclarationNode) properties[i];
                    elements[i++] = property.IsNative
                        ? GetNativeTypeRef(property.Type)
                        : GetTypeRef(property.Type);
                }

                type.SetBody(false, elements);
                
                GenerateDefaultConstructor(type, node);
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
        return Box(GetTypeRef("std::Integer"), _context.CreateConstant(node.Value));
    }

    public Value VisitMemberAccessNode(MemberAccessNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitMethodDeclarationNode(MethodDeclarationNode node)
    {
        switch (_currentGenerationPhase)
        {
            case Phase.CreateMethods:
            {
                Visit(node.Prototype);

                node.Symbol.CustomTargetData = node.Prototype.Symbol.CustomTargetData;
                break;
            }

            case Phase.VisitMethodBodies:
            {
                IrFunction func = (IrFunction) node.Symbol.CustomTargetData;
                BasicBlock block = func.AppendBasicBlock("entry");
                _instructionBuilder.PositionAtEnd(block);
                Visit(node.Prototype);
                _instructionBuilder.Return(func.ReturnType.GetNullValue());
                break;
            }
        }

        return null;
    }

    public Value VisitMethodPrototypeNode(MethodPrototypeNode node)
    {
        switch (_currentGenerationPhase)
        {
            case Phase.CreateMethods:
                IFunctionType retType
                    = _context.GetFunctionType((ITypeRef) ((MethodSymbol) node.Symbol).Type.CustomTargetData);
                node.Symbol.CustomTargetData = _module.CreateFunction(node.Symbol.GetPath(), retType);
                break;
            
            case Phase.VisitMethodBodies:
                foreach (AstNode n in node.Children)
                {
                    Visit(n);
                }

                break;
        }

        return null;
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
        // Called only for local variables

        Alloca ptr = _instructionBuilder.Alloca(GetTypeRef(node.Type));
        ptr.Name = node.Name;

        if (node.Value != null)
        {
            _instructionBuilder.Store(_instructionBuilder.Load(Visit(node.Value)), ptr);
        }
        
        return ptr;
    }

    public Value VisitTypeReferenceNode(TypeReferenceNode node)
    {
        throw new InvalidOperationException();
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