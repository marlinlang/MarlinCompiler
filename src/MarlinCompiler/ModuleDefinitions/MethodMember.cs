namespace MarlinCompiler.ModuleDefinitions;

public class MethodMember : ITypeMember
{
    public string Name { get; }
    public MethodOverload[] Overloads { get; }
    public MemberVisibility Visibility { get; }

    public MethodMember(string name, MethodOverload[] overloads, MemberVisibility visibility)
    {
        Name = name;
        Overloads = overloads;
        Visibility = visibility;
    }
}