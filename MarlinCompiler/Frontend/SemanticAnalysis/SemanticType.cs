namespace MarlinCompiler.Frontend.SemanticAnalysis;

/// <summary>
/// Represents a type declaration.
/// </summary>
public class SemanticType
{
    public SemanticType(string name, string[] genericParams)
    {
        Name          = name;
        GenericParams = genericParams;
    }

    /// <summary>
    /// The name of the type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The generic parameters that this type takes.
    /// </summary>
    public string[] GenericParams { get; }
}