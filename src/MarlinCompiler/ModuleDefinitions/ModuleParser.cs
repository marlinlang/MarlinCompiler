using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MarlinCompiler.ModuleDefinitions;

public static class ModuleParser
{
    public static ModuleDefinition Parse(string path)
    {
        try
        {
            using (StreamReader file = File.OpenText(path))
            {
                using (JsonTextReader reader = new(file))
                {
                    JObject mainObject = (JObject) JToken.ReadFrom(reader);

                    string name = (string) mainObject["moduleName"];
                    string author = (string) mainObject["moduleAuthor"];
                    List<IModuleType> types = new();

                    foreach (JObject obj in (JArray) mainObject["types"])
                    {
                        types.Add(ParseType(obj));
                    }
                    
                    return new ModuleDefinition(1, name, author, types.ToArray());
                }
            }
        }
        catch (JsonSerializationException ex)
        {
            Console.WriteLine($"Could not parse module {path}: {ex.Message}");
            if (Debugger.IsAttached)
            {
                throw;
            }
            return null;
        }
    }
    
    private static IModuleType ParseType(JObject obj)
    {
        string name = (string) obj["typeName"];
        bool canCreate = (bool) obj["canCreate"];
        MemberVisibility visibility = GetVisibility((string) obj["visibility"]);
        ITypeMember[] members = ParseMembers((JArray)obj["members"]);

        JObject data = (JObject) obj["data"];
        
        switch ((string) obj["typeKind"])
        {
            case "class":
            {
                bool isInheritable = (bool) data["inheritable"];
                bool isStatic = (bool) data["static"];
                JArray basesJson = (JArray) data["bases"];
                string[] bases = new string[basesJson.Count];
                for (int i = 0; i < bases.Length; i++)
                {
                    bases[i] = (string) basesJson[i];
                }
                return new ClassType(name, visibility, canCreate, members, isInheritable, isStatic, bases);
            }
            
            default:
                throw new NotImplementedException();
        }
    }

    private static ITypeMember[] ParseMembers(JArray members)
    {
        List<ITypeMember> parsed = new();
        foreach (JObject obj in members)
        {
            string name = (string) obj["name"];
            MemberVisibility visibility = GetVisibility((string) obj["visibility"]);
            
            switch ((string) obj["kind"])
            {
                case "method":
                {
                    List<MethodOverload> overloads = new();
                    foreach (JObject overload in (JArray) obj["overloads"])
                    {
                        bool isStatic = (bool) overload["static"];
                        string returns = (string) overload["returns"];
                        JArray signatureJson = (JArray) overload["signature"];
                        string[] signature = new string[signatureJson.Count];
                        for (int i = 0; i < signature.Length; i++)
                        {
                            signature[i] = (string) signatureJson[i];
                        }
                        overloads.Add(new MethodOverload(signature, returns, isStatic));
                    }
                    parsed.Add(new MethodMember(name, overloads.ToArray(), visibility));

                    break;
                }
            }
        }

        return parsed.ToArray();
    }

    private static MemberVisibility GetVisibility(string vis) => vis switch
    {
        "public" => MemberVisibility.Public,
        "internal" => MemberVisibility.Internal,
        "private" => MemberVisibility.Private,
        _ => throw new NotImplementedException()
    };
}