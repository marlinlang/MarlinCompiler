namespace MarlinCompiler.Common;

/// <summary>
/// A record that represents a location in a file.
/// </summary>
/// <param name="Path">The path to the file.</param>
/// <param name="Line">The line of the location.</param>
/// <param name="Col">The column of the line of the location.</param>
public record struct FileLocation(string Path, int? Line = null, int? Col = null)
{
    public override string ToString()
    {
        return $"{Path} on Line {Line}:{Col}";
    }
}