using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using MarlinCompiler.MarlinCompiler.Compilation;

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
        _builder.Messages.Error(
            msg,
            new FileLocation(
                _builder.CurrentFile,
                line,
                charPositionInLine
            )
        );
    }
}

public class ErrorListener<T> : IAntlrErrorListener<T>
{
    private IBuilder _builder;

    public ErrorListener(IBuilder builder)
    {
        _builder = builder;
    }

    public void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        _builder.Messages.Error(
            msg,
            new FileLocation(
                _builder.CurrentFile,
                line,
                charPositionInLine
            )
        );
    }
}