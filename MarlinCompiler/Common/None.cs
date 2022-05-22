namespace MarlinCompiler.Common;

/// <summary>
/// Used as a no-void-in-generics workaround. 
/// </summary>
public sealed record None
{
    /// <summary>
    /// Shorthand non-null-safe <see cref="None"/>. Behind the scenes, it is quite literally:
    /// <code>null!</code>
    /// Useful to return from a method that returns a <see cref="None"/>.
    /// </summary>
    public static readonly None Null = null!;
    
    private None()
    {
        throw new InvalidOperationException();
    }
}