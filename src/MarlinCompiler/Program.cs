using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.Reflection;
using MarlinCompiler.Compilation;

namespace MarlinCompiler;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.ResetColor();
        return CreateCommands().Invoke(args);
    }

    private static RootCommand CreateCommands()
    {
        RootCommand command = new("Marlin Compiler")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        command.Add(CreateCompileCommand());

        return command;
    }

    private static Command CreateCompileCommand()
    {
        Command command = new("compile", "Compile a project")
        {
            new Argument<string>(
                "path",
                "The path of the directory or file to compile."
            ),
            new Option(
                "--mdk",
                "The path to the MDK.",
                typeof(string),
                () => Environment.GetEnvironmentVariable("MARLINMDKPATH") ?? "MARLINMDKPATH environment variable",
                ArgumentArity.ExactlyOne
            )
        };
        
        command.Handler = CommandHandler.Create(Compile);
        
        return command;
    }

    private static void Compile(string path, string mdk)
    {
        if (!File.Exists(mdk) && !Directory.Exists(mdk))
        {
            mdk = null;
        }
        
        path = Path.GetFullPath(path);

        IBuilder builder = new Builder();
        bool success = builder.Build(path);

        Console.Write("Compilation ");
        Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write($"{(success ? "SUCCESSFUL" : "FAILED")}");
        Console.ResetColor();
        Console.WriteLine(".");
        if (builder.Messages.Count > 0)
        {
            Console.WriteLine($"{builder.Messages.Count} message{(builder.Messages.Count == 1 ? "" : "s")}:");
            builder.Messages.Contents.ForEach(message =>
            {
                Console.ForegroundColor = message.Level switch
                {
                    CompileMessageLevel.Error => ConsoleColor.Red,
                    CompileMessageLevel.Message => ConsoleColor.White,
                    CompileMessageLevel.Warning => ConsoleColor.Yellow
                };
                Console.WriteLine(message.ToString());
                Console.ResetColor();
            });
        }
    }
}