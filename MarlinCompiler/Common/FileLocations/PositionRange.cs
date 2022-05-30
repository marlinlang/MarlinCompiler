namespace MarlinCompiler.Common.FileLocations;

/// <summary>
/// Represents a range of characters in a file.
/// </summary>
/// <param name="Start">The start of the range.</param>
/// <param name="End">The end of the range.</param>
public record PositionRange(FilePosition Start, FilePosition End)
{
    /// <summary>
    /// Single character position.
    /// </summary>
    public PositionRange(FilePosition characterPosition) : this(characterPosition, characterPosition)
    {
    }
    
    public override string ToString()
    {
        return $"Line {Start.Line}:{Start.Column}";
    }
}