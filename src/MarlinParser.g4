parser grammar MarlinParser;

options { tokenVocab=MarlinLexer; }

// Top level definition
file
    : moduleName
        typeDeclaration*
    ;

// Fragments
typeName
    : IDENTIFIER (DOUBLE_COLON IDENTIFIER)*
    | NATIVE IDENTIFIER
    ;

moduleName
    : MODULE typeName SEMICOLON
    ;

modifier
    : PRIVATE
    | PUBLIC
    | STATIC
    | READONLY
    | SEALED
    ;

expectArgs
    : LPAREN (expectArg (COMMA expectArg)*)? RPAREN
    ;

expectArg
    : typeName IDENTIFIER
    ;

giveArgs
    : LPAREN (expression (COMMA expression)*)? RPAREN
    ;

// Base rules
statement
    : methodCall                SEMICOLON
    | localVariableDeclaration  SEMICOLON
    | variableAssignment        SEMICOLON
    | return                    SEMICOLON
    ;

expression
    : LPAREN expression RPAREN
    | methodCall
    | memberAccess
    | booleanLiteral
    | stringLiteral
    | INTEGER
    | DOUBLE
    ;

// Containers (types and methods)
typeDeclaration
    : classDeclaration
    | structDeclaration
    ;

classDeclaration
    : modifier* CLASS IDENTIFIER (COLON typeName (COMMA typeName)*)? LBRACE classMember* RBRACE
    ;

structDeclaration
    : modifier* STRUCT IDENTIFIER LBRACE structMember* RBRACE
    ;

methodDeclaration
    : modifier* typeName IDENTIFIER methodBody
    ;

// Subrules (statements and expressions)
methodCall
    : memberAccess giveArgs
    ;

variableDeclaration
    : modifier* typeName IDENTIFIER (ASSIGN expression)?
    ;

localVariableDeclaration
    : typeName IDENTIFIER (ASSIGN expression)?
    ;

variableAssignment
    : memberAccess ASSIGN expression
    ;

memberAccess
    // std::Console.WriteLine
    : (typeName DOT)? IDENTIFIER (LBRACKET expression RBRACKET)?
    // x.Parent.Node.Something
    | memberAccess DOT memberAccess
    // x
    | IDENTIFIER (LBRACKET expression RBRACKET)?
    ;

return
    : RETURN expression?
    ;

// Bodies
classMember // this is expected to have more coming!
    : variableDeclaration       SEMICOLON
    | methodDeclaration
    ;

structMember
    : variableDeclaration       SEMICOLON
    | methodDeclaration
    ;

methodBody
    : expectArgs ARROW expression SEMICOLON
    | expectArgs LBRACE statement* RBRACE
    ;

// Complex literals
booleanLiteral
    : TRUE
    | FALSE
    ;

stringLiteral
    : NORMAL_STRING
    ;