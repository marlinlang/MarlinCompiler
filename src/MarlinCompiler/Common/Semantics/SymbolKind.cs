namespace MarlinCompiler.Common.Semantics;

public enum SymbolKind
{
    Module,
    ClassType,
    StructType,
    ExternedType,
    Variable,
    VariableReference, // reference to var, must have AccessInstance to the variable
    Method,
    StaticProperty,
    StaticMethod,
    ExternedMethod,
}