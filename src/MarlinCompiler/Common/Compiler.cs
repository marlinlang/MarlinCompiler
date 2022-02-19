using System.Collections.Concurrent;
using System.Diagnostics;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Frontend;
using MarlinCompiler.Intermediate;
using TokenType = MarlinCompiler.Frontend.TokenType;

namespace MarlinCompiler.Common;

/// <summary>
/// Main class for the compilation process.
/// </summary>
public sealed class Compiler
{
    /// <summary>
    /// All compilation messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    /// <summary>
    /// The options for this compilation.
    /// </summary>
    private readonly CompilationOptions _options;
    
    /// <summary>
    /// Internal list of file paths for compilation.
    /// </summary>
    private readonly List<string> _filePaths;

    /// <summary>
    /// The root path of the project.
    /// </summary>
    private readonly string _rootPath;

    public Compiler(string root, CompilationOptions options)
    {
        _rootPath = Path.GetFullPath(root);
        _options = options;
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
    /// Method for starting the compilation process.
    /// </summary>
    /// <returns>Program exit code.</returns>
    public int Compile()
    {
        ContainerNode program = FrontendCompilation();
        program = IntermediateCompilation(program);

        if (!MessageCollection.HasFatalErrors)
        {
            BackendCompilation(program);
        }

        PrintMessages();

        // return code:
        //   no messages:               0
        //   warnings/info messages:    100
        //   fatal errors:              200
        return GetReturnCode();
    }

    #region Compilation process

    /// <summary>
    /// Performs lexing, parsing and semantic analysis on all the files. 
    /// </summary>
    /// <returns>The program under an unified node.</returns>
    private ContainerNode FrontendCompilation()
    {
        ConcurrentBag<CompilationUnitNode> compilationUnits = new();

        Parallel.ForEach(_filePaths, path =>
        {
            FileParser parser = new FileParser(Lex(path), path);
            
            CompilationUnitNode unit = parser.Parse();
            if (unit != null)
            {
                compilationUnits.Add(unit);
            }
            
            MessageCollection.AddRange(parser.MessageCollection);
        });
        
        // Combine roots of compilation units
        ContainerNode root = new();
        foreach (CompilationUnitNode compilationUnit in compilationUnits)
        {
            root.Children.AddRange(compilationUnit.Children);
        }

        return root;
    }

    private ContainerNode IntermediateCompilation(ContainerNode root)
    {
        SemanticAnalyzer analyzer = new();
        // TODO: analyzer.Analyze(root);
        MessageCollection.AddRange(analyzer.MessageCollection);
        
        return root;
    }
    
    private void BackendCompilation(ContainerNode program)
    {
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
            ? 200
            : MessageCollection.Count() > 0
                ? 100
                : 0;
    }

    /// <summary>
    /// Prints the MessageCollection to the console.
    /// </summary>
    private void PrintMessages()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        foreach (Message msg in MessageCollection)
        {
            Console.ForegroundColor = msg.PrintColor;

            string location = (msg.Location?.ToString() + ": ") ?? "";
            if (!_options.HasFlag(CompilationOptions.UseAbsolutePaths))
            {
                // Truncate paths
                if (location.StartsWith(_rootPath))
                {
                    location = location.Substring(_rootPath.Length);
                }
            }
            
            Console.WriteLine(location + msg.Fatality switch
                              {
                                  MessageFatality.Severe => "error",
                                  MessageFatality.Warning => "warn",
                                  MessageFatality.Information => "info",
                                  _ => throw new InvalidOperationException()
                              }
                              + ": " + msg.Content);
        }

        string failedPassed = MessageCollection.HasFatalErrors ? "failed" : "successful";
        Console.ForegroundColor = MessageCollection.HasFatalErrors ? ConsoleColor.Red : ConsoleColor.Green;
        int count = MessageCollection.Count();
        if (count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"Build {failedPassed} with {count} message{(count == 1 ? "" : 's')}");
        }
        else
        {
            Console.WriteLine($"Build {failedPassed} with no messages");
        }
        
        Console.ResetColor();
    }

    /// <summary>
    /// Performs lexical analysis for a file.
    /// </summary>
    private Tokens Lex(string path)
    {
        Lexer.Token[] tokens = new Lexer(File.ReadAllText(path), path).Lex();
        foreach (Lexer.Token tok in tokens.Where(x => x.Type == TokenType.Invalid))
        {
            MessageCollection.Error($"Unknown token '{tok.Value}'", tok.Location);
        }

        return new Tokens(tokens);
    }

    #endregion
}