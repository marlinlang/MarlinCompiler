using MarlinCompiler.Ast;
using MarlinCompiler.Symbols;
using Ubiquity.NET.Llvm.DebugInfo;
using Ubiquity.NET.Llvm.Instructions;
using Ubiquity.NET.Llvm.Interop;
using Ubiquity.NET.Llvm.Types;
using Ubiquity.NET.Llvm.Values;

namespace MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;

public partial class LlvmCompilationTarget
{
    private Value? _voidValue = null;

    private Value VoidValue => Box(GetTypeRef("std::Void"), null);

    private ITypeRef GetTypeRef(string name) => _module.GetTypeByName(name);

    private ITypeRef GetTypeRef(TypeReferenceNode node)
    {
        ITypeRef tRef = (ITypeRef) node.Symbol.CustomTargetData;

        return node.IsArray ? tRef.CreatePointerType() : tRef;
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
        switch (ty.ToString())
        {
            case "i1":
                return _context.CreateConstant(false);
            case "i32":
                return _context.CreateConstant(0);
        }
        
        switch (((IStructType) ty).Name)
        {
            case "std::Void":
                return VoidValue;
            case "std::Integer":
                return Box(ty, _context.CreateConstant(0));
            case "std::Character":
                return Box(ty, _context.CreateConstant(0));
            case "std::Double":
                return Box(ty, _context.CreateConstant(0d));
            default:
                return ty.GetNullValue();
        }
    }

    private uint GetPropertyIndex(TypeDeclarationNode type, string propertyName)
    {
        uint current = type is StructDeclarationNode ? 0u : 1u;
        foreach (AstNode member in type.TypeBody.Children)
        {
            if (member is VariableDeclarationNode varDecl)
            {
                if (varDecl.Name == propertyName)
                {
                    return current;
                }
                
                current++;
            }
        }

        throw new ArgumentException("Cannot find the given property", "propertyName");
    }
    
    private void GenerateDefaultConstructor(ITypeRef type, TypeDeclarationNode decl)
    {
        IFunctionType funcTy = _context.GetFunctionType(_context.VoidType, type.CreatePointerType());
        IrFunction func = _module.CreateFunction(decl.Symbol.GetPath() + ".ctor", funcTy);

        BasicBlock entryBlock = func.AppendBasicBlock("entry");
        _instructionBuilder.PositionAtEnd(entryBlock);

        foreach (VariableDeclarationNode prop in decl.TypeBody.Children.Where(x => x is VariableDeclarationNode))
        {
            ITypeRef propType;

            if (prop.IsNative)
            {
                propType = GetNativeTypeRef(prop.Type);
            }
            else
            {
                propType = (ITypeRef) prop.Type.Symbol.CustomTargetData;
            }

            Value gep = _instructionBuilder.GetStructElementPointer(
                type,
                func.Parameters[0],
                GetPropertyIndex(decl, prop.Name)
            );
            Value defaultValue;
            if (prop.Type.IsArray)
            {
                defaultValue = CreateArray(
                    propType,
                    Box(
                        GetTypeRef("std::Integer"),
                        _context.CreateConstant(0)
                    )
                );
            }
            else
            {
                // get def value for native type todo
                defaultValue = GetDefaultValue(propType);
            }
            _instructionBuilder.Store(
                prop.Type.IsArray
                    ? propType.CreatePointerType().GetNullValue()
                    : propType.GetNullValue(),
                gep
            );
        }
        
        _instructionBuilder.Return();
    }

    private void CallDefaultConstructor(IStructType ty, IPointerType ptr)
    {
        _module.TryGetFunction(ty.Name + ".ctor", out IrFunction func);
        _instructionBuilder.Call(func);
    }
    
    private Value Box(ITypeRef targetType, Value? insertValue)
    {
        Alloca ptr = _instructionBuilder.Alloca(targetType);
        ptr.Name = "boxPtr";

        if (insertValue != null)
        {
            _instructionBuilder.Store(
                insertValue,
                _instructionBuilder.GetStructElementPointer(targetType, ptr, 0)
            );
        }
        
        return ptr;
    }

    private Value Unbox(ITypeRef targetType, Value ptr)
    {
        return _instructionBuilder.GetStructElementPointer(targetType, ptr, 0);
    }

    private Value CreateArray(ITypeRef elementType, Value elementCount)
    {
        elementCount = Unbox(((IPointerType) elementCount.NativeType).ElementType, elementCount);
        elementCount = _instructionBuilder.Load(elementCount);

        // Get size of elementType
        // https://stackoverflow.com/a/30830445/13580938
        Value size = _instructionBuilder.GetElementPtr(
            elementType,
            elementType.CreatePointerType().GetNullValue(),
            new Value[] { _context.CreateConstant(1) }
        );
        Value sizeI = _instructionBuilder.PointerToInt(size, _context.Int32Type);
        
        Value arrayMalloc = _instructionBuilder.Call(
            _cMalloc,
            _instructionBuilder.Mul(
                sizeI,
                elementCount
            )
        );

        return _instructionBuilder.IntToPointer(arrayMalloc, elementType.CreatePointerType());
    }
}