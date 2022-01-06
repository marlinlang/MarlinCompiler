using System.Text;
using MarlinCompiler.Ast;
using MarlinCompiler.Symbols;

namespace MarlinCompiler.ModuleDefinitions;

public static class ModuleDefinitionBuilder
{
    public static void Create(string path, string moduleName, RootBlockNode root)
    {
        int nestLevel = 0;
        
        StringBuilder jsonBuilder = new();
        jsonBuilder.AppendLine($"{Nest(nestLevel)}{{");
        nestLevel++;
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"mnmdVersion\": 1,");
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"moduleName\": \"{moduleName}\",");
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"moduleAuthor\": \"???\",");
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"types\": [");
        nestLevel++;
        
        // Types
        foreach (TypeDeclarationNode type in root.Body)
        {
            LoadType(type, jsonBuilder, ref nestLevel, type != root.Body.Last());
        }
        
        nestLevel--;
        jsonBuilder.AppendLine($"{Nest(nestLevel)}]");
        nestLevel--;
        jsonBuilder.AppendLine($"{Nest(nestLevel)}}}");
        
        File.WriteAllText(path, jsonBuilder.ToString());
    }

    private static void LoadType(TypeDeclarationNode type, StringBuilder jsonBuilder, ref int nestLevel, bool addComma)
    {
        jsonBuilder.AppendLine($"{Nest(nestLevel)}{{");
        nestLevel++;

        switch (type)
        {
            case ClassDeclarationNode cls:
                jsonBuilder.AppendLine($"{Nest(nestLevel)}\"typeKind\": \"class\",");
                jsonBuilder.AppendLine($"{Nest(nestLevel)}\"typeName\": \"{type.Name}\",");
                jsonBuilder.AppendLine($"{Nest(nestLevel)}\"visibility\": \"{type.Visibility.ToString().ToLower()}\",");
                jsonBuilder.AppendLine($"{Nest(nestLevel)}\"canCreate\": true,");
                jsonBuilder.AppendLine($"{Nest(nestLevel)}\"data\": {{");
                nestLevel++;
                
                jsonBuilder.AppendLine($"{Nest(nestLevel)}\"inheritable\": {(!cls.IsSealed).ToString().ToLower()},");
                jsonBuilder.AppendLine($"{Nest(nestLevel)}\"static\": {cls.IsStatic.ToString().ToLower()},");
                jsonBuilder.AppendLine($"{Nest(nestLevel)}\"bases\": [");
                nestLevel++;

                foreach (TypeReferenceNode b in cls.BaseClasses)
                {
                    jsonBuilder.Append($"{Nest(nestLevel)}\"{b.Name}\"");

                    if (b != cls.BaseClasses.Last())
                    {
                        jsonBuilder.AppendLine(",");
                    }
                    else
                    {
                        jsonBuilder.AppendLine();
                    }
                }
                
                nestLevel--;
                jsonBuilder.AppendLine($"{Nest(nestLevel)}],");
                jsonBuilder.AppendLine($"{Nest(nestLevel)}\"members\": [");
                nestLevel++;

                foreach (VariableDeclarationNode var in type.Children.Where(x => x is VariableDeclarationNode))
                {
                    LoadVarDecl(var, jsonBuilder, ref nestLevel, var != type.Children.Last());
                }

                List<string> doneOverloads = new();
                List<MethodDeclarationNode> methods = new();
                foreach (MethodDeclarationNode method in type.Children.Where(x => x is MethodDeclarationNode))
                {
                    if (!doneOverloads.Contains(method.Name))
                    {
                        doneOverloads.Add(method.Name);
                        methods.Add(method);
                    }
                }

                foreach (MethodDeclarationNode method in methods)
                {
                    LoadMethodDecl(method, jsonBuilder, ref nestLevel, method != methods.Last());
                }
                
                nestLevel--;
                jsonBuilder.AppendLine($"{Nest(nestLevel)}]");
                
                nestLevel--;
                jsonBuilder.AppendLine($"{Nest(nestLevel)}}}");
                break;
            default:
                throw new NotImplementedException();
        }
        
        nestLevel--;
        jsonBuilder.Append($"{Nest(nestLevel)}}}");

        if (addComma)
        {
            jsonBuilder.AppendLine(",");
        }
        else
        {
            jsonBuilder.AppendLine();
        }
    }

    private static void LoadVarDecl(VariableDeclarationNode var,
        StringBuilder jsonBuilder, ref int nestLevel, bool addComma)
    {
        jsonBuilder.AppendLine($"{Nest(nestLevel)}{{");
        nestLevel++;

        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"kind\": \"variable\",");
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"name\": \"{var.Name}\",");
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"visibility\": \"{var.Visibility.ToString().ToLower()}\",");
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"type\": \"{var.Type.Name}\",");
        
        nestLevel--;
        jsonBuilder.Append($"{Nest(nestLevel)}}}");
        
        if (addComma)
        {
            jsonBuilder.AppendLine(",");
        }
        else
        {
            jsonBuilder.AppendLine();
        }
    }

    private static void LoadMethodDecl(MethodDeclarationNode method,
        StringBuilder jsonBuilder, ref int nestLevel, bool addComma)
    {
        jsonBuilder.AppendLine($"{Nest(nestLevel)}{{");
        nestLevel++;

        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"kind\": \"variable\",");
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"name\": \"{method.Name}\",");
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"visibility\": \"{method.Visibility.ToString().ToLower()}\",");
        jsonBuilder.AppendLine($"{Nest(nestLevel)}\"overloads\": [");
        nestLevel++;

        Symbol[] symbols = method.Symbol.Parent.LookupMultiple(method.Name);
        foreach (MethodSymbol overload in symbols)
        {
            jsonBuilder.AppendLine($"{Nest(nestLevel)}{{");
            nestLevel++;
            
            jsonBuilder.AppendLine($"{Nest(nestLevel)}\"static\": {method.IsStatic.ToString().ToLower()},");
            jsonBuilder.AppendLine($"{Nest(nestLevel)}\"signature\": [");
            nestLevel++;

            foreach (VariableSymbol arg in overload.Args)
            {
                jsonBuilder.Append($"{Nest(nestLevel)}\"{arg.Type}\"");
                if (arg != overload.Args.Last()) jsonBuilder.Append(",");
                jsonBuilder.AppendLine();
            }
            
            nestLevel--;
            jsonBuilder.AppendLine($"{Nest(nestLevel)}],");
            jsonBuilder.AppendLine($"{Nest(nestLevel)}\"returns\": \"{overload.Type.Name}\"");
            
            nestLevel--;
            jsonBuilder.Append($"{Nest(nestLevel)}}}");
            
            if (overload != symbols.Last()) jsonBuilder.Append(",");
            jsonBuilder.AppendLine();
        }
        
        nestLevel--;
        jsonBuilder.AppendLine($"{Nest(nestLevel)}]");
        
        nestLevel--;
        jsonBuilder.Append($"{Nest(nestLevel)}}}");
        
        if (addComma)
        {
            jsonBuilder.AppendLine(",");
        }
        else
        {
            jsonBuilder.AppendLine();
        }
    }

    private static string Nest(int level) => new string('\t', level);
}