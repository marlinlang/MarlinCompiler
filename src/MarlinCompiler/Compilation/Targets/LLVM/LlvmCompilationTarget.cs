using System.Reflection;
using MarlinCompiler.Ast;
using MarlinCompiler.Compilation;
using Ubiquity.NET.Llvm;
using Ubiquity.NET.Llvm.Instructions;
using Ubiquity.NET.Llvm.Values;
using static Ubiquity.NET.Llvm.Interop.Library;

namespace MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;

public partial class LlvmCompilationTarget : BaseCompilationTarget, IDisposable
{
    private readonly IBuilder _builder;
    private readonly Context _context;
    private readonly BitcodeModule _module;
    private readonly InstructionBuilder _instructionBuilder;

    private readonly IrFunction _cMalloc;
    private readonly IrFunction _cGetChar;
    private readonly IrFunction _cPutChar;
    
    private Phase _currentGenerationPhase;

    public LlvmCompilationTarget(IBuilder builder)
    {
        _builder = builder;
        _context = new Context();
        _module = _context.CreateBitcodeModule("Program");
        _instructionBuilder = new InstructionBuilder(_context);

        //_module.TryGetFunction("getchar", out _cGetChar);
        _cMalloc = _module.CreateFunction(
            "malloc",
            _context.GetFunctionType(_context.Int32Type, _context.Int32Type)
        );
        _cGetChar = _module.CreateFunction(
            "getchar",
            _context.GetFunctionType(_context.Int32Type)
        );
        _cPutChar = _module.CreateFunction(
            "putchar",
            _context.GetFunctionType(_context.Int32Type, _context.Int32Type)
        );
    }

    public override bool InvokeTarget(AstNode root, string outPath)
    {
        using (InitializeLLVM())
        {
            foreach (Phase phase in Enum.GetValues(typeof(Phase)))
            {
                _currentGenerationPhase = phase;
                Visit(root);
            }

            if (!_module.Verify(out string verifyErr))
            {
                Messages.Error(verifyErr);
            }

            if (!_module.WriteToTextFile(outPath, out string writeErr))
            {
                Messages.Error(writeErr);
            }
        }

        return true;
    }

    public void Dispose()
    {
        _context.Dispose();
        _module.Dispose();
    }
}