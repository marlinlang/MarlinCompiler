using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;

namespace MarlinCompiler.Compilation;

public class ErrorListener : BaseErrorListener
{
    private IBuilder _builder;

    public ErrorListener(IBuilder builder)
    {
        _builder = builder;
    }

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
        int charPositionInLine, string msg, RecognitionException e)
    {
        _builder.Messages.AddError(
            msg,
            new FileLocation(
                _builder.CurrentFile,
                line,
                charPositionInLine
            )
        );
    }
}