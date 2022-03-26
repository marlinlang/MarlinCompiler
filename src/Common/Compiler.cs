using System.Collections.Concurrent;
using MarlinCompiler.Backend;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Frontend;
using MarlinCompiler.Frontend.Lexing;
using SemanticAnalyzer = MarlinCompiler.Frontend.SemanticAnalysis.SemanticAnalyzer;

namespace MarlinCompiler.Common;

/// <summary>
/// Main class for the compilation process.
/// </summary>
public sealed class Compiler
{
    public Compiler(string rootPath, string? outPath = null)
    {
        MessageCollection = new MessageCollection();
        _filePaths = new List<string>();

        if (!File.Exists(rootPath) && !Directory.Exists(rootPath))
        {
            throw new IOException($"Cannot find path for project {rootPath}");
        }
        
        _outPath = outPath ?? Path.Combine(Path.GetDirectoryName(rootPath)!, "out/"); 

        string? mdkPath = Environment.GetEnvironmentVariable("MDK");
        if (mdkPath != null)
        {
            LoadFilePaths(mdkPath);
        }
        
        LoadFilePaths(rootPath);
    }
    
    /// <summary>
    /// All compilation messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    /// <summary>
    /// Internal list of file paths for compilation.
    /// </summary>
    private readonly List<string> _filePaths;
    
    /// <summary>
    /// The out path for the project.
    /// </summary>
    private readonly string _outPath;

    /// <summary>
    /// Method for starting the compilation process.
    /// </summary>
    /// <returns>Program exit code.</returns>
    public int Compile(bool doNotInvokeLlvm)
    {
        ContainerNode program = Parse(); /* Lex & parse       */
        Analyze(program);                /* Semantic analysis */

        if (!MessageCollection.HasFatalErrors && !doNotInvokeLlvm)
        {
            Build(program);              /* LLVM compilation  */
        }

        
        /*
         * Status             Return code
         *    NO MESSAGES     0
         *    WARNINGS/INFOS  1
         *    FATAL ERRORS    2
         */
        
        return GetReturnCode();
    }

    #region Compilation process

    /// <summary>
    /// Performs lexing and parsing on all the files.
    /// </summary>
    /// <returns>The program under an unified node.</returns>
    private ContainerNode Parse()
    {
        ContainerNode root = new();
        
        ConcurrentBag<CompilationUnitNode> compilationUnits = new();

        Parallel.ForEach(_filePaths, path =>
        {
            Tokens tokens = Lex(path);

            if (tokens.ContainsInvalid) return;
            
            Parser parser = new(tokens, path);
            
            CompilationUnitNode unit = parser.Parse();
            compilationUnits.Add(unit);
            
            MessageCollection.AddRange(parser.MessageCollection);
        });

        foreach (CompilationUnitNode unit in compilationUnits)
        {
            bool found = false;
            foreach (Node node in root)
            {
                CompilationUnitNode existingUnit = (CompilationUnitNode) node;
                if (existingUnit.FullName != unit.FullName) continue;
                
                existingUnit.Children.AddRange(unit);
                found = true;
                break;
            }

            if (!found)
            {
                root.Children.Add(unit);
            }
        }

        return root;
    }

    /// <summary>
    /// Invokes the semantic analyzer for the given root.
    /// </summary>
    private void Analyze(ContainerNode root)
    {
        SemanticAnalyzer analyzer = new(root);
        analyzer.Analyze();
        MessageCollection.AddRange(analyzer.MessageCollection);
    }
    
    /// <summary>
    /// Invokes LLVM tools to build the program.
    /// </summary>
    private void Build(ContainerNode program)
    {
        OutputBuilder builder = new(program, _outPath);
        builder.BuildLlvm();
        MessageCollection.AddRange(builder.MessageCollection);
    }

    #endregion

    #region Utilities

    /// <summary>
    /// This method adds .mn files to the internal list of file paths for compilation.
    /// </summary>
    /// <param name="fromRoot">The directory to inspect.
    /// Its subdirectories will be included as well.</param>
    private void LoadFilePaths(string fromRoot)
    {
        fromRoot = Path.GetFullPath(fromRoot);
        
        // Handle path is a file
        if (File.Exists(fromRoot))
        {
            if (Path.GetExtension(fromRoot) == ".mn")
            {
                _filePaths.Add(fromRoot);
            }

            return;
        }
        
        // Handle directories
        foreach (string dir in Directory.GetFileSystemEntries(fromRoot))
        {
            LoadFilePaths(dir);
        }
    }

    /// <summary>
    /// Returns the appropriate return code.
    /// </summary>
    private int GetReturnCode()
    {
        return MessageCollection.HasFatalErrors
            ? 2
            : MessageCollection.Any()
                ? 1
                : 0;
    }

    /// <summary>
    /// Performs lexical analysis for a file.
    /// </summary>
    private Tokens Lex(string path)
    {
        Lexer lexer = new(File.ReadAllText(path), path);
        Lexer.Token[] tokens = lexer.Lex();
        MessageCollection.AddRange(lexer.MessageCollection);

        return new Tokens(tokens);
    }

    #endregion
}