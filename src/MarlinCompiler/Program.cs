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
            new Argument<string>(
                "path",
                "The path to the file or directory for compilation"
            ),
            new Option(
                "--verbose",
                "Paths for errors will be absolute instead of relative"
            )
        };

        command.Handler = CommandHandler.Create(CompilationHandler);

        return command.Invoke(args);
    }

    private static int CompilationHandler(string path, bool verbose)
    {
        Compiler compiler = new Compiler(path);
        
        int returnCode = compiler.Compile();
        int msgCount = compiler.MessageCollection.Count();
        bool passed = compiler.MessageCollection.HasFatalErrors;
        
        Console.WriteLine($"Build {passed} with {msgCount} message{(msgCount == 1 ? "" : 's')}");
        
        foreach (Message msg in compiler.MessageCollection)
        {
            Console.ForegroundColor = msg.PrintColor;
            string location = (msg.Location?.ToString() + ": ") ?? "";
            
            // Shorter file paths
            if (!verbose && location.StartsWith(path))
            {
                location = location.Substring(path.Length);
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
        
        return returnCode;
    }
}