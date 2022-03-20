namespace MarlinCompiler.Common;

/// <summary>
/// Used as a no-void-in-generics workaround. 
/// </summary>
public sealed class None
{
    private None()
    {
        throw new InvalidOperationException();
    }
}