﻿namespace MarlinCompiler.Frontend;

/// <summary>
/// This enum represents the type of a lexer token.
/// </summary>
public enum TokenType
{
    // Keywords
    Module,             // module
    Class,              // class
    Struct,             // struct
    Using,              // using
    New,                // new
    Modifier,           // public, private, protected, internal, static, readonly 
    
    // Numbers
    String,             // String literals
    Character,          // Character literals
    Boolean,            // Boolean literals
    Decimal,            // Decimal numbers
    Integer,            // Whole numbers
    
    // Operators
    And,                // &&
    Or,                 // ||
    DoubleColon,        // ::
    Assign,             // =
    Equal,              // ==
    NotEqual,           // !=
    Plus,               // -
    Minus,              // -
    Asterisk,           // *
    Slash,              // /
    Colon,              // :
    Question,           // ?
    Ampersand,          // &
    Comma,              // ,
    Dot,                // .
    Semicolon,          // ;
    
    // Braces
    LeftParen,          // (
    RightParen,         // )
    LeftBrace,          // {
    RightBrace,         // }
    LeftBracket,        // [
    RightBracket,       // ]
    LeftAngle,          // <
    RightAngle,         // >
    
    // Special
    Skip,               // Comments
    Invalid,            // Unmatched tokens
    Identifier,         // Names
}