grammar myGrammar;

program : expr EOF;

expr : addition;

addition : multiplication ((ADD | SUB) multiplication)*;

multiplication : unary ((mul | DIV) unary)*;

unary : SUB? unarysufix+;

unarysufix : power suffix*;
suffix : FAC;

power : atom (CARET atom)*;
power_nofunc : atom_nofunc (CARET atom_nofunc)*;

atom : parenthesis | number | function | variable | abs | intfunc | frac | sqrt;
atom_nofunc : parenthesis | number | variable | abs | intfunc | frac | sqrt;

function : FUNC_NAME subexpr? supexpr? DERIVATIVE* (func_args | power_nofunc) | letter subexpr? supexpr? DERIVATIVE* func_args;
func_args : L_PAREN expr? (COMMA expr)* R_PAREN; // power or unary

abs : BAR expr BAR;
intfunc : (L_FLOOR | L_CEIL) expr (R_FLOOR | R_CEIL); // floor, ceil or round
frac : FRAC_CMD L_BRACE expr R_BRACE L_BRACE expr R_BRACE;
sqrt : SQRT_CMD (L_BRACKET expr R_BRACKET)? L_BRACE expr R_BRACE;

subexpr : UNDERSCORE (L_BRACE expr R_BRACE | atom);
supexpr : CARET (L_BRACE expr R_BRACE | atom);

parenthesis : L_PAREN expr R_PAREN
            | L_BRACE expr R_BRACE
            | L_BRACKET expr R_BRACKET;

number : DIGIT+ ('.' DIGIT*)? ;//(escient NEG? DIGIT+)?;  

variable : letter;
letter : CHAR | LATEX_CMD;

WS : [ \n\t\r]+ -> skip;

DIGIT : [0-9];
CHAR : [a-zA-Z];

CARET : '^';
COMMA : ',';
UNDERSCORE : '_';

FRAC_CMD : '\\frac';
SQRT_CMD : '\\sqrt';

DERIVATIVE : '\'';

// add/sub
ADD : '+';
SUB : '-';

// mul/div
mul : MUL | CDOT | TIMES;
MUL : '*';
CDOT : '\\cdot';
TIMES : '\\times';
DIV : '/';

//// pow
//escient : 'e' | 'E';

// parenthesis
L_PAREN : '(' | '\\left(';
R_PAREN : ')' | '\\right)';
L_BRACE : '{';
R_BRACE : '}';
L_BRACKET : '[';
R_BRACKET : ']';
BAR : '|';
L_FLOOR : '\\lfloor';
R_FLOOR : '\\rfloor';
L_CEIL : '\\lceil';
R_CEIL : '\\rceil';

// factorial
FAC : '!';

// functions
FUNC_NAME : FUNC_NAME_LATEX | '\\' FUNC_NAME_LATEX | 'abs' | 'floor' | 'ceil' | 'round';
FUNC_NAME_LATEX : 'sin' | 'cos' | 'tan' | 'cot' | 'sec' | 'csc' | 'log' | 'ln' | 'exp';

LATEX_CMD : '\\' [a-zA-Z]+;