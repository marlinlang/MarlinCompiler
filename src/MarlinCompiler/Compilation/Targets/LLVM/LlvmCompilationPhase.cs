namespace MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;

public partial class LlvmCompilationTarget
{
    private enum Phase
    {
        /// <summary>
        /// Create the definitions for the types, but don't give them bodies
        /// </summary>
        CreateTypes             = 0,
        
        /// <summary>
        /// Create the methods, but don't give them bodies
        /// </summary>
        CreateMethods           = 1,
        
        /// <summary>
        /// Create virtual tables and default constructors.
        /// </summary>
        FinalizeTypes           = 3,
        
        /// <summary>
        /// Visit method bodies, i.e. build the rest of the program.
        /// </summary>
        VisitMethodBodies       = 4
    }
}