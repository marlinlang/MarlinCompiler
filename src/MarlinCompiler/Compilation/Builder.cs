using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MarlinCompiler.Antlr;
using MarlinCompiler.Ast;
using MarlinCompiler.MarlinCompiler.Antlr;
using MarlinCompiler.MarlinCompiler.Compilation;
using MarlinCompiler.MarlinCompiler.Compilation.Targets;
using MarlinCompiler.MarlinCompiler.Compilation.Targets.LLVM;
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
        List<MarlinParser.FileContext> parsed = new();
        foreach (string file in files)
        {
            MarlinParser.FileContext parsedFile = Parse(file);
            if (parsedFile == null) continue;
            parsed.Add(parsedFile);
        }
        if (Messages.HasErrors) return false;

        foreach (MarlinParser.FileContext context in parsed)
        {
            root.Body.AddRange(((RootBlockNode)astGenerator.VisitFile(context)).Body);
        }
        Messages.LoadMessages(astGenerator.Messages);
        if (Messages.HasErrors) return false;

        // Create root symbol
        root.Symbol = new RootSymbol();

        // Type fixing
        TypeFixer fixer = new(this);
        fixer.Visit(root);
        Messages.LoadMessages(fixer.Messages);
        if (Messages.HasErrors) return false;
        
        // Semantic analysis
        SemanticAnalyzer analyzer = new(this);
        analyzer.Visit(root);
        Messages.LoadMessages(analyzer.Messages);
        if (Messages.HasErrors) return false;
        
        // Semantic checking
        SemanticChecker checker = new(this);
        checker.Visit(root);
        Messages.LoadMessages(checker.Messages);
        if (Messages.HasErrors) return false;
        
        // Target invocation
        BaseCompilationTarget target = new LlvmCompilationTarget();
        target.InvokeTarget(root);
        
        return !Messages.HasErrors;
    }

    private MarlinParser.FileContext Parse(string path)
    {
        using StreamReader reader = new(path);

        AntlrInputStream inputStream = new(reader);
        MarlinLexer lexer = new(inputStream, DisregardTextWriter.Use, DisregardTextWriter.Use);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new ErrorListener<int>(this));
            
        CommonTokenStream tokenStream = new(lexer);
        MarlinParser parser = new(tokenStream, DisregardTextWriter.Use, DisregardTextWriter.Use);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ErrorListener(this));
        //parser.ErrorHandler = new BailErrorStrategy();
            
        CurrentFile = path;
        return parser.file();
    }

    private string[] GetFilePaths(string super)
    {
        // super directory is a file itself
        if (File.Exists(super))
        {
            return new[] { super };
        }

        List<string> paths = new();

        foreach (string path in Directory.GetFileSystemEntries(super))
        {
            paths.AddRange(GetFilePaths(path));
        }
        
        return paths.ToArray();
    }
}