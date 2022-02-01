namespace MarlinCompiler.Common.Symbols;

/// <summary>
/// Represents a variable assignment.
/// </summary>
public class VariableAssignmentSymbol : Symbol
{
    /// <summary>
    /// The variable being assigned to.
    /// </summary>
    public VariableSymbol? Variable { get; }
    
    /// <summary>
    /// The value that's being assigned.
    /// </summary>
    public Symbol Value { get; }

    public VariableAssignmentSymbol(VariableSymbol? variable, Symbol value) : base("var.assign")
    {
        Variable = variable;
        Value = value;
    }
}