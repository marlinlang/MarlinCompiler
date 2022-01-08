lexer grammar MarlinLexer;

WHITESPACES: (BlockComment|SingleLineComment|[ \n\t\r])+               -> channel(HIDDEN);

MODULE:         'module';
CLASS:          'class';
STRUCT:         'struct';
RETURN:         'return';
NEW:            'new';
NATIVE:         'native';
NULL:           'null';
TRUE:           'true';
FALSE:          'false';

PRIVATE:        'private';
INTERNAL:       'internal';
PUBLIC:         'public';
STATIC:         'static';
READONLY:       'readonly';
SEALED:         'sealed';

AT:             '@';
DOT:            '.';
COMMA:          ',';
COLON:          ':';
SEMICOLON:      ';';
LBRACE:         '{';
RBRACE:         '}';
LPAREN:         '(';
RPAREN:         ')';
LBRACKET:       '[';
RBRACKET:       ']';
ASSIGN:         '=';
QUOTE:          '\'';
DOUBLE_QUOTE:   '"';
DOUBLE_COLON:   '::';
ARROW:          '=>';

IDENTIFIER:              ([A-Za-z_])([0-9A-Za-z_]*);
INTEGER:                 WholeNumber | HexNumber;
NORMAL_STRING:           DOUBLE_QUOTE (~["])* DOUBLE_QUOTE;
CHARACTER:               QUOTE (EscapeSequence|~[']) QUOTE;
DOUBLE:                  DoubleNumber;

fragment WholeNumber
    : [-+]?[0-9]+
    ;

fragment HexNumber
    : '0x' WholeNumber
    ;

fragment DoubleNumber
    : WholeNumber '.' [0-9]+
    | [-+]? '.' [0-9]+
    ;

fragment SingleLineComment
    : '//' ~[\r\n]*
    ;

fragment BlockComment
    : '/*'  .*? '*/'
    ;

fragment Character
    : EscapeSequence
    | .
    ;

fragment EscapeSequence
    : '\\' [btnfr"'\\]
    ;