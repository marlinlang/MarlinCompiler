using MarlinCompiler.Symbols;

namespace MarlinCompiler.ModuleDefinitions;

public static class SymbolBuilder
{
    public static RootSymbol CreateTree(ModuleDefinition def)
    {
        RootSymbol symbol = new();

        foreach (IModuleType type in def.Types)
        {
            AddType(type, symbol);
        }
        
        return symbol;
    }

    private static void AddType(IModuleType type, RootSymbol sym)
    {
        switch (type)
        {
            case ClassType cls:
            {
                ClassTypeSymbol symbol = new(type.Name, type.Visibility, cls.IsStatic,
                        !cls.IsInheritable, cls.Bases);

                foreach (ITypeMember member in type.Members)
                {
                    AddMember(member, symbol);
                }

                sym.AddChild(symbol);
                return;
            }
            
            default:
                throw new NotImplementedException(type.GetType().Name);
        }
    }

    private static void AddMember(ITypeMember member, TypeSymbol sym)
    {
        switch (member)
        {
            case MethodMember method:
            {
                foreach (MethodOverload overload in method.Overloads)
                {
                    List<Symbol> args = new();

                    int i = 0;
                    foreach (string arg in overload.Signature)
                    {
                        args.Add(new VariableSymbol($"arg{i++}", arg));
                    }
                    
                    MethodSymbol symbol = new(method.Name, overload.IsStatic, method.Visibility, args);
                }
                
                break;
            }
            
            default:
                throw new NotImplementedException(member.GetType().Name);
        }
    }
}