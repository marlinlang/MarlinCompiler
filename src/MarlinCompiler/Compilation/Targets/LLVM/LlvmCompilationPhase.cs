namespace MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;

public partial class LlvmCompilationTarget
{
    private enum Phase
    {
        CreateTypes             = 0,
        CreateMethods           = 1,
        CreateProperties        = 2,
        CreateVirtualTables     = 3,
        VisitMethodBodies       = 4
    }
}