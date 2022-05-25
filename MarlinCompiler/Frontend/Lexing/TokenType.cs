namespace MarlinCompiler.Frontend.Lexing;

/// <summary>
/// This enum represents the type of a lexer token.
/// </summary>
public enum TokenType
{
    // Keywords
    Module,      // module
    Class,       // class
    Struct,      // struct
    Constructor, // constructor
    Using,       // using
    New,         // new
    Operator,    // operator
    Get,         //get
    Set,         // set
    Mutable,     // mut
    Void,        // void
    Extern,      // extern
    Return,      // return
    Modifier,    // public, private, protected, internal, static, readonly 

    // Numbers
    String,    // String literals
    Character, // Character literals
    Boolean,   // Boolean literals
    Decimal,   // Decimal numbers
    Integer,   // Whole numbers

    // Operators
    Arrow,       // ->
    And,         // &&
    Or,          // ||
    DoubleColon, // ::
    Assign,      // =
    Equal,       // ==
    NotEqual,    // !=
    At,          // @
    Power,       // ^
    Plus,        // -
    Minus,       // -
    Asterisk,    // *
    Slash,       // /
    Colon,       // :
    Question,    // ?
    Ampersand,   // &
    Comma,       // ,
    Dot,         // .
    Semicolon,   // ;

    // Braces
    LeftParen,  // (
    RightParen, // )
    LeftBrace,  // {
    RightBrace, // }
    LeftAngle,  // <
    RightAngle, // >

    // Special
    Skip,      // Comments
    Invalid,   // Unmatched tokens
    Identifier // Names
}