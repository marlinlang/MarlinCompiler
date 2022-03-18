using System.Collections.Concurrent;
using System.Diagnostics;
using MarlinCompiler.Backend;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Frontend;
using TokenType = MarlinCompiler.Frontend.TokenType;

namespace MarlinCompiler.Common;

/// <summary>
/// Main class for the compilation process.
/// </summary>
public sealed class Compiler
{
    public Compiler(string root)
    {
        _rootPath = Path.GetFullPath(root);
        MessageCollection = new MessageCollection();
        _filePaths = new List<string>();

        string? mdkPath = Environment.GetEnvironmentVariable("MDK");
        if (mdkPath != null)
        {
            LoadFilePaths(mdkPath);
        }
        
        LoadFilePaths(root);
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
    /// The root path of the project.
    /// </summary>
    private readonly string _rootPath;

    /// <summary>
    /// Method for starting the compilation process.
    /// </summary>
    /// <returns>Program exit code.</returns>
    public int Compile()
    {
        ContainerNode program = Parse();
        Analyze(program);

        if (!MessageCollection.HasFatalErrors)
        {
            Build(program);
        }

        // return code:
        //   no messages:               0
        //   warnings/info messages:    1
        //   fatal errors:              2
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
            if (unit != null)
            {
                compilationUnits.Add(unit);
            }
            
            MessageCollection.AddRange(parser.MessageCollection);
        });

        foreach (CompilationUnitNode unit in compilationUnits)
        {
            foreach (ContainerNode child in unit)
            {
                root.Children.Add(child);
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
    /// <param name="program"></param>
    private void Build(ContainerNode program)
    {
        OutputBuilder builder = new(program, _rootPath);
        builder.BuildLlvm();
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
            : MessageCollection.Count() > 0
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