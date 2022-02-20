namespace MarlinCompiler.Common.Semantics;

public enum SymbolKind
{
    Module,
    ClassType,
    StructType,
    ExternedType,
    Variable,
    Method,
    StaticProperty,
    StaticMethod,
    ExternedMethod,
}