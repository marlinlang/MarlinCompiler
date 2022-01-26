parser grammar MarlinParser;

options { tokenVocab=MarlinLexer; }

// Top level definition
file
    : (moduleDefinition typeDeclaration*)?
        EOF
    ;

moduleDefinition
    : MODULE IDENTIFIER (DOUBLE_COLON IDENTIFIER)*
    ;

typeDeclaration
    : MODIFIER* CLASS IDENTIFIER typeBody SEMICOLON         #ClassTypeDeclaration
    | MODIFIER* STRUCT IDENTIFIER typeBody SEMICOLON        #StructTypeDeclaration
    | MODIFIER* INTERFACE IDENTIFIER typeBody SEMICOLON     #InterfaceTypeDeclaration
    ;

typeBody
    : ((variableDeclaration|methodDeclaration) SEMICOLON)*
    ;
    
variableDeclaration
    : MODIFIER* typeName (ASSIGN expression)?
    ;

methodDeclaration
    : MODIFIER* typeName IDENTIFIER LPAREN (typeName IDENTIFIER (COMMA typeName IDENTIFIER)*)? RPAREN
    ;

typeName
    : (IDENTIFIER (DOUBLE_COLON IDENTIFIER)) name=IDENTIFIER (LANGLE typeName RANGLE)
    ;

expression
    // x.y.z is allowed by this rule
    // if we included it in member we'd get LL(*) trouble :(
    : expression DOT expression     #UnfoldExpression
    
    | (
        literal
      | methodCall
      | 
      )                             #ValueExpression
    ;

methodCall
    : member LPAREN (expression (COMMA expression)*)? RPAREN
    ;

member
    : typeName DOT IDENTIFIER           #TypeMemberAccess
    | variableAccess DOT IDENTIFIER     #VariableMemberAccess
    ;
    
variableAccess
    : IDENTIFIER
    ;

literal
    : STRING        #StringLiteral
    | INTEGER       #IntegerLiteral
    | DOUBLE        #DoubleLiteral
    | CHARACTER     #CharacterLiteral
    ;