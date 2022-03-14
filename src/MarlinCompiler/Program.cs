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
        Compiler compiler = new(path);

        int returnCode = compiler.Compile();
        int msgCount = compiler.MessageCollection.Count();
        bool fail = compiler.MessageCollection.HasFatalErrors;
        
        // Crazy output time
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Marlin build ");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(fail ? "failed" : "successful");
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($" with {msgCount} message{(msgCount == 1 ? "" : 's')}{(msgCount == 0 ? "" : ':')}");
        
        foreach (Message msg in compiler.MessageCollection)
        {
            string location = msg.Location?.ToString() ?? "";
            
            // Shorter file paths
            if (!verbose && location.StartsWith(path))
            {
                location = location.Substring(path.Length);
            }

            string fatality = msg.Fatality switch
            {
                MessageFatality.Severe => "ERROR",
                MessageFatality.Warning => "WARN",
                MessageFatality.Information => "INFO",
                _ => throw new InvalidOperationException()
            };

            if (location != "")
            {
                Console.ForegroundColor = msg.PrintColor;
                Console.Write(fatality);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" at ");
                
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(location);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(":");
            }
            else
            {
                Console.ForegroundColor = msg.PrintColor;
                Console.Write(fatality);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(":");
            }
            
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(msg.Content);
            
            Console.WriteLine();
        }
        
        return returnCode;
    }
}