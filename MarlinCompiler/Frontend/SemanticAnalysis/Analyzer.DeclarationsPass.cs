using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Symbols.Kinds;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

public partial class Analyzer
{
    private class DeclarationsPass : NoNieVisitor
    {
        public DeclarationsPass(Analyzer analyzer)
        {
            _analyzer = analyzer;
        }

        private readonly Analyzer _analyzer;

        public override None MethodDeclaration(MethodDeclarationNode node)
        {
            Visit(node.Type);
            ((MethodSymbol) node.GetMetadata<SymbolTable>().PrimarySymbol!).ReturnType
                = new TypeUsageSymbol(node.Type.GetMetadata<TypeSymbol>());

            foreach (VariableNode parameter in node.Parameters)
            {
                Visit(parameter.Type);
            }

            foreach (Node statement in node)
            {
                Visit(statement);
            }

            return None.Null;
        }

        public override None Property(PropertyNode node)
        {
            Visit(node.Type);

            return None.Null;
        }

        public override None LocalVariable(LocalVariableDeclarationNode node)
        {
            Visit(node.Type);
            node.GetMetadata<VariableSymbol>().Type = new TypeUsageSymbol(node.Type.GetMetadata<TypeSymbol>());

            return None.Null;
        }

        public override None ClassDefinition(ClassTypeDefinitionNode node)
        {
            foreach (Node member in node)
            {
                Visit(member);
            }

            return None.Null;
        }

        public override None ExternTypeDefinition(ExternTypeDefinitionNode node)
        {
            foreach (Node member in node)
            {
                Visit(member);
            }

            return None.Null;
        }

        public override None StructDefinition(StructTypeDefinitionNode node)
        {
            foreach (Node member in node)
            {
                Visit(member);
            }

            return None.Null;
        }

        public override None TypeReference(TypeReferenceNode node)
        {
            SemanticUtils.SetTypeRefMetadata(_analyzer, node);

            return None.Null;
        }
    }
}