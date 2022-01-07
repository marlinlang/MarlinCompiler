using System.Reflection;
using MarlinCompiler.Ast;
using Ubiquity.NET.Llvm;
using Ubiquity.NET.Llvm.Instructions;
using static Ubiquity.NET.Llvm.Interop.Library;

namespace MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;

public partial class LlvmCompilationTarget : BaseCompilationTarget, IDisposable
{
    private readonly Context _context;
    private readonly BitcodeModule _module;
    private readonly InstructionBuilder _instructionBuilder;

    private Phase _currentGenerationPhase;

    public LlvmCompilationTarget()
    {
        _context = new Context();
        _module = _context.CreateBitcodeModule("Program");
        _instructionBuilder = new InstructionBuilder(_context);
    }

    public override bool InvokeTarget(AstNode root)
    {
        using (InitializeLLVM())
        {
            foreach (Phase phase in Enum.GetValues(typeof(Phase)))
            {
                _currentGenerationPhase = phase;
                Visit(root);
            }

            Console.WriteLine(_module.WriteToString());
        }

        return true;
    }

    public void Dispose()
    {
        _context.Dispose();
        _module.Dispose();
    }
}