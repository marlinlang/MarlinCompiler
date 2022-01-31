using System.Diagnostics;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Frontend;
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

    public Compiler(string root, CompilationOptions options)
    {
        _options = options;
        MessageCollection = new MessageCollection();
        _filePaths = new List<string>();
        LoadFilePaths(root);
    }

    /// <summary>
    /// Method for starting the compilation process.
    /// </summary>
    /// <returns>Program exit code.</returns>
    public int Compile()
    {
        FrontendCompilation();
        IntermediateCompilation();

        if (!MessageCollection.HasFatalErrors)
        {
            BackendCompilation();
        }

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        foreach (Message msg in MessageCollection)
        {
            Console.ForegroundColor = msg.PrintColor;
            Console.WriteLine(msg.ToString());
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

        // return code:
        //   no messages:               0
        //   warnings/info messages:    100
        //   fatal errors:              200
        return MessageCollection.HasFatalErrors
            ? 200
            : MessageCollection.Count() > 0
                ? 100
                : 0;
    }

    #region Frontend

    private void FrontendCompilation()
    {
        Dictionary<string, Tokens> lexed = new();

        // Lexing
        foreach (string path in _filePaths)
        {
            lexed[path] = Lex(path);
        }
        
        // Parsing
        List<CompilationUnitNode> compilationUnits = new();
        foreach ((string path, Tokens tokens) in lexed)
        {
            FileParser parser = new FileParser(tokens, path);
            CompilationUnitNode node = parser.Parse();
            if (node != null)
            {
                compilationUnits.Add(node);
            }

            MessageCollection.AddRange(parser.MessageCollection);
        }
    }

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

    #region Intermediate

    private void IntermediateCompilation()
    {
    }

    #endregion

    #region Backend

    private void BackendCompilation()
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

    #endregion
}