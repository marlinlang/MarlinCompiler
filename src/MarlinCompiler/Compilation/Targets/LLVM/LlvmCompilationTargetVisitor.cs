using MarlinCompiler.Ast;
using MarlinCompiler.Compilation;
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
        return Box(GetTypeRef("std::Boolean"), _context.CreateConstant(node.Value));
    }

    public Value VisitClassDeclarationNode(ClassDeclarationNode node)
    {
        switch (_currentGenerationPhase)
        {
            case Phase.CreateTypes:
            {
                if (!node.IsStatic)
                {
                    IStructType type = new DebugStructType(_module, node.Name, null, node.Name);
                    node.Symbol.CustomTargetData = type;
                }

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
                if (!node.IsStatic)
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
                }

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

                for (int i = 0; i < properties.Length; i++)
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
            case Phase.VisitMethodBodies:
            {
                Visit(node.Prototype);

                node.Symbol.CustomTargetData = node.Prototype.Symbol.CustomTargetData;
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
                List<ITypeRef> args = new();

                if (!((MethodSymbol) node.Symbol).IsStatic)
                {
                    ITypeRef type = null;
                    Symbol useSymbol = node.Symbol;
                    while (type == null)
                    {
                        if (useSymbol is TypeSymbol t)
                        {
                            type = GetTypeRef(t.Name);
                            break;
                        }

                        useSymbol = useSymbol.Parent;
                    }
                    args.Add(_context.GetPointerTypeFor(type));
                }
                
                IFunctionType funcType = _context.GetFunctionType(
                    _context.GetPointerTypeFor(
                        (ITypeRef) ((MethodSymbol) node.Symbol).Type.CustomTargetData
                    ),
                    args.ToArray()
                );
                
                node.Symbol.CustomTargetData = _module.CreateFunction(node.Symbol.GetPath(), funcType);
                break;
            
            case Phase.VisitMethodBodies:
                IrFunction func = (IrFunction) node.Symbol.CustomTargetData;
                BasicBlock block = func.AppendBasicBlock("entry");
                _instructionBuilder.PositionAtEnd(block);
                
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
        if (node.IsNative)
        {
            // First argument is the invoking method
            // Rest are the arguments

            AstNode[] givenArgs = node.Args.Skip(1).ToArray();

            string nativeFuncName = ((StringNode) node.Args[0]).Value;
            switch (nativeFuncName)
            {
                case "box":
                {
                    return Box(GetTypeRef(((StringNode) givenArgs[0]).Value), Visit(givenArgs[1]));
                }
                case "unbox":
                {
                    return Unbox(GetTypeRef(((StringNode) givenArgs[0]).Value), Visit(givenArgs[1]));
                }
                case "c_getchar":
                {
                    return _instructionBuilder.Call(_cGetChar);
                }
                case "c_putchar":
                {
                    Value arg = Visit(givenArgs[0]);
                    return _instructionBuilder.Call(_cPutChar, _instructionBuilder.Load(arg));
                }
                default:
                {
                    Messages.Error($"Unknown native func {nativeFuncName}",
                        new FileLocation(_builder, node.Context.Start)
                    );
                    break;
                }
            }
        }

        List<Value> args = new();
        foreach (AstNode arg in node.Args)
        {
            args.Add(Visit(arg));
        }

        if (!((MethodSymbol) node.Symbol).IsStatic)
        {
            // Add 0th arg `this`
            
        }

        return _instructionBuilder.Call((Value) node.Symbol.CustomTargetData, args.ToArray());
    }

    public Value VisitReturnNode(ReturnNode node)
    {
        if (node.Value == null)
        {
            return _instructionBuilder.Return(VoidValue);
        }
        else
        {
            return _instructionBuilder.Return(Visit(node.Value));
        }
    }

    public Value VisitStringNode(StringNode node)
    {
        throw new NotImplementedException();
    }

    public Value VisitCharacterNode(CharacterNode node)
    {
        return Box(GetTypeRef("std::Character"), _context.CreateConstant((int)node.Value));
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
        ITypeRef elementType = GetTypeRef(node.ArrayType);
        Value elementCount = Visit(node.ElementCount);

        elementCount = Unbox(elementCount.NativeType, elementCount);

        return _instructionBuilder.BitCast(
            _instructionBuilder.Call(
                _cMalloc,
                _instructionBuilder.Mul(
                    _context.CreateConstant(elementType.IntegerBitWidth),
                    elementCount
                )
            ),
            elementType
        );
    }
}