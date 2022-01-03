lexer grammar MarlinLexer;

channels { COMMENTS_CHANNEL }

SINGLE_LINE_COMMENT:     '//'  .*?                -> channel(COMMENTS_CHANNEL);
BLOCK_COMMENT:           '/*'  .*? '*/'           -> channel(COMMENTS_CHANNEL);

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
DOUBLE_QUOTE:   '"';
DOUBLE_COLON:   '::';
ARROW:          '=>';

IDENTIFIER:              ([A-Za-z_])([0-9A-Za-z_]*);
INTEGER:                 WholeNumber | HexNumber;
NORMAL_STRING:           DOUBLE_QUOTE ~('"')* DOUBLE_QUOTE;
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

WHITESPACES:             [ \n\t\r]+               -> channel(HIDDEN);