﻿using MarlinCompiler.Common.Semantics.Symbols;
using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// Method calls are both expressions and statements.
/// </summary>
public class MethodCallNode : IndexableExpressionNode
{
    public string MethodName { get; }
    public ExpressionNode[] Args { get; }
    
    public MethodDeclarationSymbol? DeclarationSymbol { get; set; }
    
    /// <summary>
    /// True for calls to LLVM functions instead of Marlin-defined ones
    /// </summary>
    public bool IsNativeCall { get; }

    public MethodCallNode(string methodName, bool isNativeCall, ExpressionNode[] args)
    {
        MethodName = methodName;
        IsNativeCall = isNativeCall;
        Args = args;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.MethodCall(this);
    }
}