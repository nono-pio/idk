grammar Latex;
//  antlr4 Latex.g4  -Dlanguage=CSharp -no-listener -visitor

// sets
// {1, 2, 3}, { expr | cond }, \R, \N, \Q, \Z, Union, Inter, A - B ou A/B
set : unions;

unions : intersections (UNION intersections)*;
intersections : differences (INTERSECTION differences)*;
differences : set_atom ((SUB | DIV) differences)?;
set_atom : finite_set | number_set | conditionnal_set | parenthesis_set |;
finite_set : L_BRACE_SET expr? (COMMA expr)* R_BRACE_SET;
number_set : R_SET | N_SET | Z_SET | Q_SET | EMPTY_SET;
conditionnal_set : L_BRACE_SET expr BAR condition R_BRACE_SET;
parenthesis_set : L_PAREN set R_PAREN;

UNION : '\\cup';
INTERSECTION : '\\cap';
N_SET : '\\N';
Z_SET : '\\Z';
Q_SET : '\\Q';
R_SET : '\\R';
EMPTY_SET : '\\empty';

// conditions
// True, False, And, Or, expr =/</<=/>/>=/!= expr, variable \in Set, \not

condition : or;
boolean : or;

or : and (OR and)*;
and : not (AND not)*;
not : NOT* cond_atom;
cond_atom : bool_value | relationnal | in_set | parenthesis_cond |;
bool_value : TRUE | FALSE;
relationnal : expr REL_SIGN expr;
in_set : variable IN set;
parenthesis_cond : L_PAREN condition R_PAREN;

OR : 'or' | 'ou' | '\\lor' | '\\vee';
AND : 'and' | 'et' | '\\land' | '\\wedge';
NOT : '\\neg';
TRUE : 'true' | 'vrai';
FALSE : 'false' | 'faux';
REL_SIGN : '=' | '<' | '>' | '<=' | '>=' | '!=' | '\\leq' | '\\geq' | '\\neq' | '\\ne';
IN : '\\in';

// expressions
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

function : FUNC_NAME subexpr? supexpr? DERIVATIVE* (func_args | power_nofunc) | letter subexpr? supexpr? DERIVATIVE* func_args; // TODO : sup = r
func_args : L_PAREN expr? (COMMA expr)* R_PAREN;

abs : BAR expr BAR;
intfunc : (L_FLOOR | L_CEIL) expr (R_FLOOR | R_CEIL); // floor, ceil or round
frac : FRAC_CMD L_BRACE expr R_BRACE L_BRACE expr R_BRACE;
sqrt : SQRT_CMD (L_BRACKET expr R_BRACKET)? L_BRACE expr R_BRACE;

subexpr : UNDERSCORE (L_BRACE expr R_BRACE | atom);
supexpr : CARET (L_BRACE expr R_BRACE | atom);

parenthesis : L_PAREN expr R_PAREN
            | L_BRACE expr R_BRACE
            | L_BRACKET expr R_BRACKET;

number : DIGIT+ ('.' DIGIT*)? | INFTY;

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
L_BRACE_SET : '\\left{';
R_BRACE_SET : '\\right}';
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

INFTY : '\\infty';

LATEX_CMD : '\\' [a-zA-Z]+;