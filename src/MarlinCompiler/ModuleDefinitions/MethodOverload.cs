namespace MarlinCompiler.ModuleDefinitions;

public class MethodOverload
{
    public string[] Signature { get; }
    public string ReturnType { get; }
    public bool IsStatic { get; }

    public MethodOverload(string[] signature, string returnType, bool isStatic)
    {
        Signature = signature;
        ReturnType = returnType;
        IsStatic = isStatic;
    }
}