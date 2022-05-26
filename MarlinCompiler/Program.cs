using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using MarlinCompiler.Common.Messages;
using MarlinCompiler.Mbf;

namespace MarlinCompiler;

/// <summary>
/// Entry class for the Marlin Compiler.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        Console.ResetColor();

        Command compileCommand = new("compile", "Compile Marlin source code.")
        {
            new Argument<string>(
                "path",
                "The path to the file or directory for compilation"
            ),
            new Option(
                "--verbose",
                "Paths for errors will be absolute instead of relative"
            ),
            new Option(
                "--analyze-only",
                "The compiler will only analyze the files and not build a program."
            )
        };
        compileCommand.Handler = CommandHandler.Create(CompilationHandler);

        RootCommand rootCommand = new("Marlin Compiler");
        rootCommand.AddCommand(compileCommand);
        return rootCommand.Invoke(args);
    }

    private static int CompilationHandler(string path, bool verbose, bool analyzeOnly)
    {
        Compiler compiler = new(path);

        int returnCode = compiler.Compile(analyzeOnly);

        if (returnCode == 3)
        {
            // No stdlib
            PrintNoStdLib();
            return returnCode;
        }
        else
        {
            PrintMessages(compiler.MessageCollection, verbose, path);
        }

        return returnCode;
    }

    private static void PrintNoStdLib()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Marlin build failed with FATAL ERROR: ");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("No standard library found!");

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("This usually indicates a broken compiler installation. Please reinstall the Marlin compiler.");

        Console.ResetColor();
    }

    private static void PrintMessages(MessageCollection collection, bool verbose, string projectPath)
    {
        int msgCount = collection.Count();
        bool fail = collection.HasFatalErrors;

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Marlin build ");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(fail ? "failed" : "successful");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($" with {msgCount} message{(msgCount == 1 ? "" : 's')}{(msgCount == 0 ? "" : ':')}");

        foreach (Message msg in collection)
        {
            string location = msg.Location?.ToString() ?? "";

            // Shorter file paths
            if (!verbose
                && location.StartsWith(projectPath))
            {
                location = location[projectPath.Length ..];
            }

            string fatality = msg.Fatality switch
            {
                MessageFatality.Severe      => "ERROR",
                MessageFatality.Warning     => "WARN",
                MessageFatality.Information => "INFO",
                _                           => throw new InvalidOperationException()
            };

            if (location != "")
            {
                Console.ForegroundColor = msg.PrintColor;
                Console.Write(fatality);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" MN{(int) msg.Id}");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" at ");

                Console.ForegroundColor = ConsoleColor.Yellow;
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

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("   | " + msg.Content.Replace("\n", "\n   : "));
        }
    }
}