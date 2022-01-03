namespace MarlinCompiler.ModuleDefinitions;

public interface IModuleType
{
    public string Name { get; }
    public MemberVisibility Visibility { get; }
    
    /// <summary>
    /// Can the type be created?
    /// </summary>
    public bool CanCreate { get; }
    
    public ITypeMember[] Members { get; }
}