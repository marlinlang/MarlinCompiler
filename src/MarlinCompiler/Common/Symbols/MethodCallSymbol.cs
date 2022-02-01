using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols;

public sealed class MethodCallSymbol : Symbol
{
    /// <summary>
    /// The owner of the method.
    /// </summary>
    public Symbol? Target { get; set; }

    /// <summary>
    /// The symbol of the method.
    /// </summary>
    public MethodSymbol? Method { get; }
    
    /// <summary>
    /// Signature of the method call
    /// </summary>
    public string Signature { get; init; }
    
    /// <summary>
    /// Was the method invoked statically?
    /// </summary>
    public bool StaticallyInvoked { get; init; }

    public MethodCallSymbol(Symbol? target, MethodSymbol? method) : base(method?.Name + ".call" ?? "<???>.call")
    {
        Target = target;
        Method = method;
        Signature = "";
        StaticallyInvoked = false;
    }
}