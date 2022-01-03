using System.Runtime.CompilerServices;
using System.Text;

namespace MarlinCompiler;

/// <summary>
/// TextWriter that does absolutely nothing.
/// </summary>
public class DisregardTextWriter : TextWriter
{
    // Was required to override
    public override Encoding Encoding { get; } = Encoding.Default;
    
    /// <summary>
    /// The only instance of DisregardTextWrite.
    /// Since this class does nothing, it's not necessary to have more than one instance of it.
    /// </summary>
    public static DisregardTextWriter Use { get; }

    private DisregardTextWriter()
    {
    }

    static DisregardTextWriter()
    {
        Use = new DisregardTextWriter();
    }
}