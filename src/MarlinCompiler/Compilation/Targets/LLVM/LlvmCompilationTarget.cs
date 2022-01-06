using Ubiquity.NET.Llvm;
using Ubiquity.NET.Llvm.Instructions;
using static Ubiquity.NET.Llvm.Interop.Library;

namespace MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;

public partial class LlvmCompilationTarget : BaseCompilationTarget
{
    private readonly Context Context;
    private readonly BitcodeModule Module;
    private readonly InstructionBuilder InstructionBuilder;

    public LlvmCompilationTarget()
    {
        Context = new Context();
        Module = Context.CreateBitcodeModule("Program");
    }

    public override bool InvokeTarget()
    {
        using (InitializeLLVM())
        {
            RegisterNative();
        }
    }
}