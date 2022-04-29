using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;
using Ubiquity.NET.Llvm.Values;

namespace MarlinCompiler.Backend;

public sealed partial class OutputBuilder : IAstVisitor<None>
{
    public None Visit(Node node)
    {
        return node.AcceptVisitor(this);
    }

    public None ClassDefinition(ClassTypeDefinitionNode node)
    {
        Emit($"{node.Accessibility.ToString().ToLower()} {(node.IsStatic ? "static " : "")}class {node.LocalName}", false);
        Emit(node.GenericTypeParamName != null ? $"<{node.GenericTypeParamName}>" : "", true);
        OpenScope();
        foreach (Node n in node)
        {
            Visit(n);
        }
        CloseScope();
        return null!;
    }

    public None StructDefinition(StructTypeDefinitionNode node)
    {
        Emit($"{node.Accessibility.ToString().ToLower()} class {node.LocalName}", true);
        OpenScope();
        foreach (Node n in node)
        {
            Visit(n);
        }
        CloseScope();
        return null!;
    }

    public None ExternedTypeDefinition(ExternedTypeDefinitionNode node)
    {
        return null!;
    }

    public None MethodDeclaration(MethodDeclarationNode node)
    {
        Emit($"{node.Accessibility.ToString().ToLower()} {(node.IsStatic ? "static " : "")}{node.Type} {node.Name}()", true);
        OpenScope();
        CloseScope();
        return null!;
    }
}