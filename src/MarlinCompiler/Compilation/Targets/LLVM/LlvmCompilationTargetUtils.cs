using MarlinCompiler.Ast;
using MarlinCompiler.Symbols;
using Ubiquity.NET.Llvm.Instructions;
using Ubiquity.NET.Llvm.Interop;
using Ubiquity.NET.Llvm.Types;
using Ubiquity.NET.Llvm.Values;

namespace MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;

public partial class LlvmCompilationTarget
{
    private Value? _voidValue = null;

    private Value VoidValue
    {
        get
        {
            if (_voidValue == null)
            {
                _voidValue = Box(GetTypeRef("std::Void"), null);
            }

            return _voidValue;
        }
    }

    private ITypeRef GetTypeRef(string name) => _module.GetTypeByName(name);

    private ITypeRef GetTypeRef(TypeReferenceNode node)
    {
        return (ITypeRef) node.Symbol.CustomTargetData;
    }

    private ITypeRef GetNativeTypeRef(TypeReferenceNode node)
    {
        return node.Name switch
        {
            "int" => _context.Int32Type,
            "char" => _context.Int16Type,
            "bool" => _context.BoolType,
            _ => throw new InvalidOperationException()
        };
    }

    private Value GetDefaultValue(ITypeRef ty)
    {
        switch (((IStructType) ty).Name)
        {
            case "std::Void":
                return VoidValue;
            case "std::Integer":
                return _context.CreateConstant(0);
            case "std::Double":
                return _context.CreateConstant(0d);
            default:
                return ty.GetNullValue();
        }
    }
    
    private void GenerateDefaultConstructor(ITypeRef type, TypeDeclarationNode decl)
    {
        IFunctionType funcTy = _context.GetFunctionType(_context.VoidType);
        IrFunction func = _module.CreateFunction(decl.Symbol.GetPath() + ".ctor", funcTy);

        BasicBlock entryBlock = func.AppendBasicBlock("entry");
        _instructionBuilder.PositionAtEnd(entryBlock);

        _instructionBuilder.Return();
    }

    private void CallDefaultConstructor(IStructType ty, IPointerType ptr)
    {
        _module.TryGetFunction(ty.Name + ".ctor", out IrFunction func);
        _instructionBuilder.Call(func);
    }
    
    private Value Box(ITypeRef targetType, Constant? insertValue)
    {
        Alloca ptr = _instructionBuilder.Alloca(targetType);

        if (insertValue != null)
        {
            _instructionBuilder.Store(
                insertValue,
                _instructionBuilder.GetStructElementPointer(targetType, ptr, 0)
            );
        }
        
        return ptr;
    }
}