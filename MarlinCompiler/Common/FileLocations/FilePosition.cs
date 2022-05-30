namespace MarlinCompiler.Common.FileLocations;

/// <summary>
/// Represents a position within a file.
/// </summary>
[Obsolete($"Use {nameof(PositionRange)} instead")]
public record FilePosition(int Line, int Column);