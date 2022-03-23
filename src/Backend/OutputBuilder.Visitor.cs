using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;
using Ubiquity.NET.Llvm.Values;

namespace MarlinCompiler.Backend;

public sealed partial class OutputBuilder : IAstVisitor<Value>
{
    public Value Visit(Node node)
    {
        return node.AcceptVisitor(this);
    }

    public Value ClassDefinition(ClassTypeDefinitionNode node)
    {
        throw new NotImplementedException();
    }
}