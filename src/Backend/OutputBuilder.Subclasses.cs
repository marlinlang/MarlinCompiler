using MarlinCompiler.Common;
using Ubiquity.NET.Llvm.DebugInfo;
using Ubiquity.NET.Llvm.Types;

namespace MarlinCompiler.Backend;

public sealed partial class OutputBuilder
{
    /// <summary>
    /// Passes for the output builder.
    /// </summary>
    private enum Pass
    {
        DefineTypes,
        DefineTypeMembers,
        MakeVtables,
        DefineTypeBodies,
        VisitTypeMembers,
    }

    /// <summary>
    /// Represents a type metadata.
    /// </summary>
    private class TypeNodeMetadata : INodeMetadata
    {
        public TypeNodeMetadata(ITypeRef type)
        {
            Type = type;
        }

        public ITypeRef Type { get; }
        public ITypeRef? Vtable { get; set; }
    }
}