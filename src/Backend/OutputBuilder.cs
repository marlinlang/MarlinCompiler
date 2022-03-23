using System.Linq.Expressions;
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
    public OutputBuilder(ContainerNode root, string path)
    {
        _root = root;
        _path = path;
        MessageCollection = new MessageCollection();

        _context = new Context();
        _instructionBuilder = new InstructionBuilder(_context);
    }

    /// <summary>
    /// The root node of the program.
    /// </summary>
    private readonly ContainerNode _root;

    /// <summary>
    /// The project path.
    /// </summary>
    private readonly string _path;

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

    #endregion

    public void BuildLlvm()
    {
        /*using (InitializeLLVM())
        {
            foreach (Pass pass in Enum.GetValues<Pass>())
            {
                _currentPass = pass;
                Visit(_root);
            }
        }*/
    }
}