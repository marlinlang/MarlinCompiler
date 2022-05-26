using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Backend;

public sealed class TypeDeclarationPass : NoNieVisitor
{
    public TypeDeclarationPass(BuilderTools tools)
    {
        _tools = tools;
    }

    private readonly BuilderTools _tools;
    
    public override None ClassDefinition(ClassTypeDefinitionNode node)
    {
        return None.Null;
    }

    public override None ExternTypeDefinition(ExternTypeDefinitionNode node)
    {
        return None.Null;
    }

    public override None StructDefinition(StructTypeDefinitionNode node)
    {
        return None.Null;
    }
}