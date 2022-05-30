namespace MarlinCompiler.Common.FileLocations;

/// <summary>
/// A record that represents a location in a file.
/// </summary>
/// <param name="Path">The path to the file.</param>
/// <param name="Range">The range of the location.</param>
[Obsolete($"Use {nameof(PositionRange)} instead")]
public record TokenLocation(string Path, PositionRange Range)
{
    public override string ToString()
    {
        return $"{Path} on {Range}";
    }
}