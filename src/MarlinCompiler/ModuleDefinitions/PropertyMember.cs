namespace MarlinCompiler.ModuleDefinitions;

public class PropertyMember : ITypeMember
{
    public string Name { get; }
    public string Type { get; }
    public bool ReadOnly { get; }

    public PropertyMember(string name, string type, bool readOnly)
    {
        Name = name;
        Type = type;
        ReadOnly = readOnly;
    }
}