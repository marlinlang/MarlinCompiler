using System.Data;
using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Extern method.
/// </summary>
public class ExternMethodSymbol : NamedSymbol
{
    public const string ConstructorTypeName = "<$constructor$>";

    public ExternMethodSymbol(ExternMethodNode node) : base(null!)
    {
        IsConstructor = node.Type.FullName == ConstructorTypeName;
        Accessibility = node.Accessibility;
        Parameters    = node.Parameters;
        Type          = null;
        IsStatic      = node.IsStatic;
        PassedArgs    = node.PassedArgs;
        _name         = node.Name;
    }

    /// <summary>
    /// Whether this method is a constructor.
    /// </summary>
    public bool IsConstructor { get; }

    /// <summary>
    /// The name of this method.
    /// </summary>
    /// <exception cref="InvalidOperationException">The method is a constructor.</exception>
    public override string Name
    {
        get
        {
            if (IsConstructor)
            {
                throw new InvalidOperationException("Cannot get name of constructor.");
            }

            if (_name == null)
            {
                throw new NoNullAllowedException("Name is somehow null.");
            }

            return _name;
        }
    }

    /// <summary>
    /// The accessibility of this method.
    /// </summary>
    public GetAccessibility Accessibility { get; }

    /// <summary>
    /// The parameters of this method.
    /// </summary>
    public VariableNode[] Parameters { get; }

    /// <summary>
    /// The type of this method.
    /// </summary>
    /// <remarks>Null by default - semantic analysis should assign. Always null for constructors.</remarks>
    public TypeUsageSymbol? Type { get; }

    /// <summary>
    /// Whether this method is static.
    /// </summary>
    public bool IsStatic { get; }

    /// <summary>
    /// The arguments that are being passed to native code.
    /// </summary>
    public ExpressionNode[] PassedArgs { get; }

    private readonly string? _name;
}