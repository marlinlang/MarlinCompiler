namespace MarlinCompiler.ModuleDefinitions;

public class ClassType : IModuleType
{
    public string Name { get; }
    public MemberVisibility Visibility { get; }
    public bool CanCreate { get; }
    public ITypeMember[] Members { get; }
    public bool IsInheritable { get; }
    public bool IsStatic { get; }
    public string[] Bases { get; }

    public ClassType(string name, MemberVisibility visibility, bool canCreate, ITypeMember[] members,
        bool isInheritable, bool isStatic, string[] bases)
    {
        Name = name;
        Visibility = visibility;
        CanCreate = canCreate;
        Members = members;
        IsInheritable = isInheritable;
        IsStatic = isStatic;
        Bases = bases;
    }
}