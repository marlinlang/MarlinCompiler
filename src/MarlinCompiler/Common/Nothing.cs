namespace MarlinCompiler.Frontend;

/// <summary>
/// Used as a no-void-in-generics workaround. 
/// </summary>
public sealed class Nothing
{
    private Nothing()
    {
        throw new InvalidOperationException();
    }
}