using Antlr4.Runtime;
using MarlinCompiler.Compilation;

namespace MarlinCompiler.MarlinCompiler.Antlr;

public class CustomErrorStrategy : BailErrorStrategy
{

    private IBuilder _builder;

    public CustomErrorStrategy(IBuilder builder)
    {
        _builder = builder;
    }

    public override void ReportError(Parser recognizer, RecognitionException e)
    {
        _builder.Messages.Error(
            e.Message,
            new FileLocation(
                _builder.CurrentFile,
                e.OffendingToken.Line,
                e.OffendingToken.Column
            )
        );
        throw e;
    }
    
    
}