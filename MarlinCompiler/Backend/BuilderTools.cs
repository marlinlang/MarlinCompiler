using Ubiquity.NET.Llvm;
using Ubiquity.NET.Llvm.Instructions;

namespace MarlinCompiler.Backend;

public sealed class BuilderTools
{
    public BuilderTools()
    {
        _modules       = new Dictionary<string, BitcodeModule>();
        _globalContext = new Context();
        _builder       = new InstructionBuilder(_globalContext);
    }

    private readonly Dictionary<string, BitcodeModule> _modules;
    private readonly Context                           _globalContext;
    private readonly InstructionBuilder                _builder;

    public BitcodeModule GetModule(string name)
    {
        return _modules[name];
    }

    public BitcodeModule CreateNewModule(string name)
    {
        BitcodeModule mod = _globalContext.CreateBitcodeModule(name);
        _modules.Add(name, mod);
        return mod;
    }
}