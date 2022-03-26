using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using Ubiquity.NET.Llvm;
using Ubiquity.NET.Llvm.Instructions;
using static Ubiquity.NET.Llvm.Interop.Library;

namespace MarlinCompiler.Backend;

/// <summary>
/// This class manages the LLVM IR generation for Marlin programs.
/// Note - called OutputBuilder to not be confused with LLVM's Builder classes
/// </summary>
public sealed partial class OutputBuilder
{
    public OutputBuilder(ContainerNode root, string outPath)
    {
        _root = root;
        _outPath = outPath;
        MessageCollection = new MessageCollection();

        _context = new Context();
        _instructionBuilder = new InstructionBuilder(_context);
        _currentModule = null!; // assigned before visiting at all
    }

    /// <summary>
    /// The root node of the program.
    /// </summary>
    private readonly ContainerNode _root;
    private readonly string _outPath;

    /// <summary>
    /// LLVM compilation messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    /// <summary>
    /// The current pass.
    /// </summary>
    private Pass _currentPass;
    
    #region LLVM-related fields

    private readonly Context _context;
    private readonly InstructionBuilder _instructionBuilder;

    private BitcodeModule _currentModule;

    #endregion

    public void BuildLlvm()
    {
        using (InitializeLLVM())
        {
            Dictionary<string, BitcodeModule> modules = new();
            foreach (Node node in _root)
            {
                CompilationUnitNode unit = (CompilationUnitNode) node;
                modules.Add(unit.FullName, _context.CreateBitcodeModule(unit.FullName));
            }
            
            foreach (Pass pass in Enum.GetValues<Pass>())
            {
                _currentPass = pass;
                foreach (Node node in _root)
                {
                    _currentModule = modules[((CompilationUnitNode) node).FullName];
                    Visit(_root);
                }
            }

            string useOutPath = _outPath;
            if (Directory.Exists(useOutPath))
            {
                new DirectoryInfo(useOutPath).Delete(true);
            }
            else if (File.Exists(useOutPath))
            {
                useOutPath = Path.GetDirectoryName(useOutPath) ?? useOutPath;
            }
            Directory.CreateDirectory(useOutPath);

            foreach ((string name, BitcodeModule mod) in modules)
            {
                if (!mod.Verify(out string verifyErr))
                {
                    MessageCollection.Error($"LLVM module failed verify check: {verifyErr}", null);
                    continue;
                }
                
                string path = Path.Combine(useOutPath, name.Replace("::", "_"));
                mod.WriteToFile(path + ".bc");
                if (!mod.WriteToTextFile(path + ".ll", out string writeErr))
                {
                    Console.WriteLine($"Could not save LL file for module {mod.Name}: {writeErr}");
                }
            }
        }
    }
}