﻿namespace MarlinCompiler.Common.Messages;

/// <summary>
/// Message numeric codes.
/// </summary>
public enum MessageId
{
    /*
     * Lexer - 1XXXX
     */
    InvalidCharacter = 1001,


    /*
     * Parser - 2XXXX
     */
    ParsingCancelled                   = 2001,
    SymbolAlreadyDefined               = 2002,
    ExpectedLlvmTypeName               = 2003,
    InapplicableModifier               = 2004,
    RepeatedModifier                   = 2005,
    InconsistentAccessibilityModifiers = 2006,
    MissingAccessibilityModifier       = 2007,
    UnexpectedToken                    = 2008,
    ExpressionNotIndexable             = 2009,


    /*
     * Semantic - 3XXXX
     */
    VariableNotFound                     = 3001,
    MemberNotFound                       = 3002,
    UnknownType                          = 3003,
    AssignedValueDoesNotMatchType        = 3004,
    StaticMethodCallOnInstance           = 3005,
    InstanceMethodCallOnTypeName         = 3006,
    GenericArgsOnNonGenericType          = 3007,
    GenericArgsDoNotMatchParams          = 3008,
    InstancePropertyAssignmentOnTypeName = 3009,
    StaticPropertyAssignmentOnInstance   = 3010,
    ArgumentCountMismatch                = 3011,
    UninitializedVariableUsage           = 3012,
    MethodCallOnVoid                     = 3013,


    /*
     * Code style - 4XXXX
     */
    RedundantPropertyAccessibilityModifier = 4001,
    AlwaysSpecifyTypeVisibility            = 4002,
    AlwaysSpecifyMemberVisibility          = 4003,
}