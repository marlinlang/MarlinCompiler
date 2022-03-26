using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Visitors;
using Ubiquity.NET.Llvm.DebugInfo;
using Ubiquity.NET.Llvm.Types;
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
        switch (_currentPass)
        {
            case Pass.DefineTypes:
                DebugStructType structType = new(
                    _currentModule,
                    $"{node.ModuleName}::{node.LocalName}",
                    _currentModule.DICompileUnit,
                    node.LocalName
                );
                node.Metadata = new TypeNodeMetadata(structType);
                break;
            
            case Pass.DefineTypeMembers:
                
                break;
            
            case Pass.MakeVtables:
                DebugStructType vtableType = new(
                    _currentModule,
                    $"vtable.{node.ModuleName}::{node.LocalName}",
                    _currentModule.DICompileUnit,
                    $"vtable.{node.LocalName}"
                );
                ((TypeNodeMetadata) node.Metadata!).Vtable = vtableType;
                break;
            
            case Pass.DefineTypeBodies:
                ((DebugStructType) ((TypeNodeMetadata) node.Metadata!).Type).SetBody(false);
                foreach (Node child in node)
                {
                    //Visit(child);
                }
                break;
            
            case Pass.VisitTypeMembers:
                foreach (Node child in node)
                {
                    //Visit(child);
                }
                break;
            
            default:
                throw new InvalidOperationException();
        }

        return null!;
    }

    public Value ExternedTypeDefinition(ExternedTypeDefinitionNode node)
    {
        switch (_currentPass)
        {
            case Pass.DefineTypes:
                if (!node.IsStatic)
                {
                    ITypeRef type = new DebugStructType(_currentModule, node.LlvmTypeName!, null,
                        $"{node.ModuleName}::{node.LocalName}");
                    node.Metadata = new TypeNodeMetadata(type);
                }

                break;
            
            case Pass.DefineTypeMembers:
                break;
            case Pass.DefineTypeBodies:
                break;
            case Pass.VisitTypeMembers:
                break;
        }
        
        return null!;
    }

    public Value StructDefinition(StructTypeDefinitionNode node)
    {
        return null!;
    }
}