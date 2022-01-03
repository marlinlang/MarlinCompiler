using Antlr4.Runtime;
using MarlinCompiler.Antlr;
using MarlinCompiler.Ast;
using MarlinCompiler.Symbols;

namespace MarlinCompiler.Compilation;

internal class Builder : IBuilder
{
    public CompileMessages Messages { get; }

    public string ProjectPath { get; set;  } = "<none>";
    
    // antlr SourceName doesn't work with the c# target for some reason
    public string CurrentFile { get; private set; } = "<none>";

    public Builder()
    {
        Messages = new CompileMessages();
    }

    public bool Build(string path)
    {
        ProjectPath = path;

        // Parsing & AST generation
        AstGenerator astGenerator = new(this);
        string[] files = GetFilePaths(path);
        RootBlockNode root = new();
        foreach (string file in files)
        {
            foreach (AstNode child in astGenerator.VisitFile(Parse(file)).Children)
            {
                root.Body.Add(child);
            }
        }
        Messages.LoadMessages(astGenerator.Messages);
        if (Messages.HasErrors) return false;

        // Type fixing
        TypeFixer fixer = new(this);
        fixer.Visit(root);
        Messages.LoadMessages(astGenerator.Messages);
        if (Messages.HasErrors) return false;
        
        // Semantic analysis
        SemanticAnalyzer analyzer = new(this);
        analyzer.Visit(root);
        Messages.LoadMessages(astGenerator.Messages);
        if (Messages.HasErrors) return false;
        
        return !Messages.HasErrors;
    }

    private MarlinParser.FileContext Parse(string path)
    {
        using StreamReader reader = new(path);
        
        AntlrInputStream inputStream = new(reader);
        MarlinLexer lexer = new(inputStream);
        CommonTokenStream tokenStream = new(lexer);
        MarlinParser parser = new(tokenStream, DisregardTextWriter.Use, DisregardTextWriter.Use);
        parser.AddErrorListener(new ErrorListener(this));

        CurrentFile = path;
        return parser.file();
    }

    private string[] GetFilePaths(string super)
    {
        // super directory is a file itself
        if (File.Exists(super))
        {
            return new[] {super};
        }

        List<string> paths = new();

        foreach (string path in Directory.GetFileSystemEntries(super))
        {
            paths.AddRange(GetFilePaths(path));
        }
        
        return paths.ToArray();
    }
}