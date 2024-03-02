grammar Math;

/*

xy -> x*y
5x -> 5*x
5sin(x) -> 5*sin(x)

*/

prog: expr EOF;

expr: add;

add: mul (op=(POS|NEG) mul)*;
mul: pow (op=(MUL|DIV) pow)*;
pow: unary (op=POW unary)?;
unary: (op=(POS|NEG))? atom;
atom: paranthesis | number | variable | function;

paranthesis: '(' expr ')' |
             '{' expr '}';
number: INT;
variable: (VAR_ID | LATEX_ID) index?;

function: func | latex_func;
func: FUNC_ID '(' expr ')';
latex_func: LATEX_ID ('{' expr '}')+;

index: '_{' expr '}';

// 
WS: [ \n\r\t]+ -> skip;

// Number
INT: [0-9]+;
FLOAT: INT ('.' INT (('e'|'E') INT)?)?;

// Unary
POS: '+';
NEG: '-';

// Separetor
ADD: POS;
SUB: NEG;
MUL: '*' | '\\cdot';
DIV: '/';
POW: '^';

// Latex variable
greek_letter: '\\' GREEK_LETTER;
GREEK_LETTER: 'alpha' 
            | 'beta'
            | 'gamma' | 'Gamma' 
            | 'delta' | 'Delta' 
            | 'epsilon' | 'varepsilon' 
            | 'zeta' 
            | 'eta' 
            | 'theta' | 'vartheta' | 'Theta' 
            | 'iota' 
            | 'kappa' 
            | 'lambda' | 'Lambda' 
            | 'mu' 
            | 'nu' 
            | 'xi' | 'Xi' 
            | 'pi' | 'Pi' 
            | 'rho' | 'varrho' 
            | 'sigma' | 'Sigma' 
            | 'tau' 
            | 'upsilon' | 'Upsilon' 
            | 'phi' | 'varphi' | 'Phi' 
            | 'chi' 
            | 'psi' | 'Psi' 
            | 'omega' | 'Omega';

// ID
VAR_ID: [a-zA-Z];
FUNC_ID: [a-zA-Z]+;
LATEX_ID: '\\' [a-zA-Z]+;