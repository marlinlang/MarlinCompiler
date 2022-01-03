namespace MarlinCompiler.ModuleDefinitions;

public sealed class ModuleDefinition
{
    public int Version { get; }
    public string ModuleName { get; }
    public string ModuleAuthor { get; }
    
    public IModuleType[] Types { get; }

    public ModuleDefinition(int version, string moduleName, string moduleAuthor, IModuleType[] types)
    {
        Version = version;
        ModuleName = moduleName;
        ModuleAuthor = moduleAuthor;
        Types = types;
    }
}