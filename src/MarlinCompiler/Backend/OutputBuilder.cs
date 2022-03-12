using MarlinCompiler.Common;

namespace MarlinCompiler.Backend;

/// <summary>
/// This class manages the LLVM IR generation for Marlin programs.
/// </summary>
public sealed partial class OutputBuilder
{
    public OutputBuilder()
    {
        MessageCollection = new MessageCollection();
    }
    
    /// <summary>
    /// LLVM compilation messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    public void BuildLlvm()
    {
        
    }
}