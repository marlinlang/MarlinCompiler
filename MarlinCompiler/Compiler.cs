using System.Collections.Concurrent;
using System.Reflection;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Messages;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Frontend.Lexing;
using MarlinCompiler.Frontend.Parsing;
using MarlinCompiler.Frontend.SemanticAnalysis;

namespace MarlinCompiler;

/// <summary>
/// Main class for the compilation process.
/// </summary>
public sealed class Compiler
{
    public Compiler(string rootPath, string? outPath = null)
    {
        MessageCollection = new MessageCollection();
        _filePaths        = new List<string>();

        if (!File.Exists(rootPath)
            && !Directory.Exists(rootPath))
        {
            throw new IOException($"Cannot find path for project {rootPath}");
        }

        _outPath = outPath ?? Path.Combine(Path.GetDirectoryName(rootPath)!, "out/");

        _filePaths.AddRange(Directory.GetFiles(rootPath, "*.mn", SearchOption.AllDirectories));
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
    public int Compile(bool analyzeOnly)
    {
        /*
         * Status             Return code
         *    NO MESSAGES     0
         *    WARNINGS/INFOS  1
         *    FATAL ERRORS    2
         *    NO STDLIB       3
         */

        if (!IsStdLibPresent())
        {
            return 3;
        }

        ContainerNode program = Parse(); /* Lex & parse       */
        Analyze(program);                /* Semantic analysis */

        if (!MessageCollection.HasFatalErrors
            && !analyzeOnly)
        {
            Build(program); /* LLVM compilation  */
        }

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
        SymbolTable rootScope = new(null);

        ConcurrentBag<CompilationUnitNode> compilationUnits = new();

        _filePaths.AddRange(Directory.GetFiles(GetStdLibPath(), "*.mn", SearchOption.AllDirectories));

        Parallel.ForEach(
            _filePaths,
            path =>
            {
                Tokens tokens = Lex(path);

                if (tokens.ContainsInvalid)
                {
                    return;
                }

                Parser parser = new(tokens, path);

                CompilationUnitNode? unit = parser.Parse();
                if (unit != null)
                {
                    compilationUnits.Add(unit);
                }

                MessageCollection.AddRange(parser.MessageCollection);
            }
        );

        foreach (CompilationUnitNode unit in compilationUnits)
        {
            bool found = false;
            foreach (Node node in root)
            {
                CompilationUnitNode existingUnit = (CompilationUnitNode) node;
                if (existingUnit.FullName != unit.FullName)
                {
                    continue;
                }

                existingUnit.Children.AddRange(unit);
                existingUnit.GetMetadata<SymbolTable>().TakeSymbolsFrom(unit.GetMetadata<SymbolTable>());
                found = true;
                break;
            }

            if (!found)
            {
                root.Children.Add(unit);
            }
        }

        foreach (Node node in root)
        {
            CompilationUnitNode unit = (CompilationUnitNode) node;
            rootScope.AddSymbol(unit.GetMetadata<SymbolTable>());
        }

        return root;
    }

    /// <summary>
    /// Invokes the semantic analyzer for the given root.
    /// </summary>
    private void Analyze(ContainerNode root)
    {
        List<CompilationUnitNode> units = root.Children.ConvertAll(x => (CompilationUnitNode) x);
        Analyzer analyzer = new(units);
        analyzer.Analyze();
        MessageCollection.AddRange(analyzer.MessageCollection);
    }

    /// <summary>
    /// Invokes LLVM tools to build the program.
    /// </summary>
    private void Build(ContainerNode program)
    {
        /*Builder builder = new(
            program.Children.Select(x => (CompilationUnitNode) x),
            _outPath
        );
        builder.Build();
        MessageCollection.AddRange(builder.MessageCollection);*/
    }

    #endregion

    #region Utilities

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

    /// <summary>
    /// Checks if the standard library is present on the system.
    /// </summary>
    /// <returns>Whether the stdlib directory exists.</returns>
    private static bool IsStdLibPresent()
    {
        /*
         * TODO: When LLVM compilation is done, this method should do more extensive checks.
         *       The stdlib should be provided in a custom package format.
         */

        return Directory.Exists(GetStdLibPath());
    }

    /// <summary>
    /// Retrieves the path to the standard library.
    /// </summary>
    private static string GetStdLibPath()
    {
        return Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(),
            "stdlib.marlin"
        );
    }

    #endregion
}