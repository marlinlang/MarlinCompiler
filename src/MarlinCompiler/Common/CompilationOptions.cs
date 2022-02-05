namespace MarlinCompiler.Common;

/// <summary>
/// Options for compilation.
/// </summary>
[Flags]
public enum CompilationOptions : short
{
    /// <summary>
    /// No special options.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// All errors must be reported with absolute paths.
    /// </summary>
    UseAbsolutePaths = 1,
}