using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.Reflection;
using MarlinCompiler.Common;

namespace MarlinCompiler;

/// <summary>
/// Entry class for the Marlin Compiler.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        Console.ResetColor();
        
        RootCommand command = new("Marlin Compiler")
        {
            new Argument<string>("path", "The path to the file or directory for compilation")
        };

        command.Handler = CommandHandler.Create(CompilationHandler);

        return command.Invoke(args);
    }

    private static int CompilationHandler(string path, bool force = false)
    {
        CompilationOptions options = CompilationOptions.None;
        return new Compiler(path, options).Compile();
    }
}