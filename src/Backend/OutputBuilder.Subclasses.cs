namespace MarlinCompiler.Backend;

public sealed partial class OutputBuilder
{
    /// <summary>
    /// Passes for the output builder.
    /// </summary>
    private enum Pass
    {
        DefineTypes,
        DefineTypeBodies,
        DefineMethods,
        MakeVtables,
        VisitTypeMembers,
    }
}