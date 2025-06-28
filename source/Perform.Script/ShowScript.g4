grammar ShowScript;

// Root rule: only consts, vars, events, and statements remain
script
    : (constStatement | eventBlock | varDeclStatement)* EOF
    ;

// Constant declaration at module level
constStatement
    : CONST ID ASSIGN expr SEMI
    ;

// C-style variable declaration (e.g. bool x = false;)
varDeclStatement
    : typeName ID ASSIGN expr SEMI
    ;

typeName
    : 'bool'
    | 'int'
    | 'float'
    | 'string'
    | 'long'
    | 'double'
    | 'Device'
    ;

// Event blocks: on(EventType(args)) while(...) when(...) { ... } loop { ... } finally { ... }
eventBlock
    : 'on' LPAREN eventTypeAndArgs RPAREN whileBlock? whenBlock? block loopBlock? loopFinallyBlock?
    ;

eventTypeAndArgs
    : eventType (',' eventArg)*
    ;

// Allowed event types
eventType
    : 'ButtonChange'
    | 'BarChange'
    | 'SectionChange'
    | 'StartSong'
    | 'EndSong'
    ;

// Event arguments: positional or named, can be expr, enum, or dottedID
eventArgs
    : (eventArg (',' eventArg)*)?
    ;

// Enum support for ButtonState
buttonStateEnum
    : 'ButtonState' '.' ('Down' | 'Up' | 'Press' | 'LongPress')
    ;

// Event argument can be expr, named expr, enum, named enum, or dottedID
eventArg
    : expr
    | ID ASSIGN expr
    | buttonStateEnum
    | ID ASSIGN buttonStateEnum
    | dottedID
    | ID ASSIGN dottedID
    ;

// Optional while block: while(condition)
whileBlock
    : 'while' LPAREN expr RPAREN
    ;

// Optional when block: when(condition)
whenBlock
    : 'when' LPAREN expr RPAREN
    ;

// Loop block and recursive loop statements
loopBlock
    : 'loop' loopStatementsBlock
    ;

// Optional finally block for eventBlock
loopFinallyBlock
    : 'finally' block
    ;

loopStatementsBlock
    : '{' (loopStatement)* '}'
    ;

// Loop statements allow recursive control flow with loop semantics
loopStatement
    : callStatement
    | assignment
    | letStatement
    | ifLoopStatement
    | forLoopStatement
    | foreachLoopStatement
    | breakStatement
    | continueStatement
    | returnStatement
    | tryLoopStatement
    | setBlockStatement
    | varDeclStatement
    | endLoopStatement
    ;

// If/Else for loop blocks (recursive)
ifLoopStatement
    : IF LPAREN expr RPAREN loopStatementsBlock (ELSE ifLoopStatement | ELSE loopStatementsBlock)?
    ;

// For loop for loop blocks (recursive)
forLoopStatement
    : FOR LPAREN (letStatementNoSemi | assignmentNoSemi | varDeclNoSemi)? SEMI expr? SEMI (letStatementNoSemi | assignmentNoSemi | varDeclNoSemi | expr)? RPAREN loopStatementsBlock
    ;

// Foreach loop for loop blocks (recursive)
foreachLoopStatement
    : FOREACH LPAREN VAR ID IN dottedID RPAREN loopStatementsBlock
    ;

// Try/catch/finally for loop blocks (recursive)
tryLoopStatement
    : TRY loopStatementsBlock catchLoopBlock finallyLoopBlock?
    ;

catchLoopBlock
    : CATCH LPAREN ID RPAREN loopStatementsBlock
    ;

finallyLoopBlock
    : FINALLY loopStatementsBlock
    ;

// Only allow endLoopStatement in loopStatement, not in general statement
endLoopStatement
    : ENDLOOP SEMI
    ;

// Action blocks (non-loop)
block
    : '{' (statement)* '}'
    ;

// Statements: device/function calls, assignment, var, if, for, foreach, try, set block
statement
    : callStatement
    | assignment
    | letStatement
    | ifStatement
    | forStatement
    | foreachStatement
    | breakStatement
    | continueStatement
    | returnStatement
    | tryStatement
    | setBlockStatement
    | varDeclStatement
    ;

// Foreach loop for regular blocks
foreachStatement
    : FOREACH LPAREN VAR ID IN dottedID RPAREN block
    ;

// Set block statement for batch device/group parameter assignment
setBlockStatement
    : 'set' LPAREN setTargets RPAREN setBlock
    ;

setTargets
    : dottedID (',' dottedID)*
    ;

setBlock
    : '{' setAssignment* '}'
    ;

setAssignment
    : ID ASSIGN expr SEMI
    ;

// JS-style var statement for variable assignment
letStatement
    : VAR ID ASSIGN expr SEMI
    ;

// Assignment (now allows dottedID on the left)
assignment
    : dottedID ASSIGN expr SEMI
    ;

// Function and device calls must end with a semicolon
callStatement
    : (songCall
    | motionCall
    | mathCall
    | colorCall
    | litesCall
    | logCall
    | genericCall
    | getCall
    | isAvailableCall
    | globalGetCall
    | globalIsAvailableCall
    ) SEMI
    ;

// Log calls: Log.Info(...), Log.Error(...), Log.Warn(...)
logCall
    : LOG_DOT logLevel '(' expr ')'
    ;

logLevel
    : INFO
    | ERROR
    | WARN
    ;

// For loop: for(init; condition; increment) { ... }
forStatement
    : FOR LPAREN (letStatementNoSemi | assignmentNoSemi | varDeclNoSemi)? SEMI expr? SEMI (letStatementNoSemi | assignmentNoSemi | varDeclNoSemi | expr)? RPAREN block
    ;

// Helper rules for for-loop initializers/increments without trailing semicolon
letStatementNoSemi
    : VAR ID ASSIGN expr
    ;

// AssignmentNoSemi (now allows dottedID on the left)
assignmentNoSemi
    : dottedID ASSIGN expr
    ;

varDeclNoSemi
    : typeName ID ASSIGN expr
    ;

// Break/Continue
breakStatement
    : BREAK SEMI
    ;

continueStatement
    : CONTINUE SEMI
    ;

// Return statement
returnStatement
    : RETURN expr? SEMI
    ;

// Try/catch/finally
tryStatement
    : TRY block catchBlock finallyBlock?
    ;

catchBlock
    : CATCH LPAREN ID RPAREN block
    ;

finallyBlock
    : FINALLY block
    ;

// Song namespace
songCall
    : 'Song' '.' songFunction
    ;

songFunction
    : 'Time' LPAREN RPAREN
    | 'Bar' LPAREN RPAREN
    | 'Beat' LPAREN RPAREN
    | 'BeatInBar' LPAREN RPAREN
    | 'Title' LPAREN RPAREN
    | 'BPM' LPAREN RPAREN
    | 'TimeSignature' LPAREN RPAREN
    ;

// Motion namespace (grouped motion control)
motionCall
    : 'Motion' LPAREN motionArgs RPAREN DOT motionFunction
    ;

motionArgs
    : dottedID (',' dottedID)*
    ;

motionFunction
    : 'Circle' LPAREN expr (',' expr)* RPAREN
    | 'Figure8' LPAREN expr (',' expr)* RPAREN
    | 'Ellipse' LPAREN expr (',' expr)* RPAREN
    ;

// Color namespace
colorCall
    : 'Color' '.' colorFunction
    ;

colorFunction
    : 'RGB' LPAREN expr ',' expr ',' expr RPAREN
    | 'RGBA' LPAREN expr ',' expr ',' expr ',' expr RPAREN
    | 'HSV' LPAREN expr ',' expr ',' expr RPAREN
    | 'Blend' LPAREN expr ',' expr ',' expr RPAREN
    | 'FromHex' LPAREN expr RPAREN
    | 'Lerp' LPAREN expr ',' expr ',' expr RPAREN
    | 'Saturate' LPAREN expr ',' expr RPAREN
    | 'Desaturate' LPAREN expr ',' expr RPAREN
    ;

// Math namespace
mathCall
    : 'Math' '.' mathFunction
    ;

mathFunction
    : 'Sin' LPAREN expr RPAREN
    | 'Cos' LPAREN expr RPAREN
    | 'Tan' LPAREN expr RPAREN
    | 'ASin' LPAREN expr RPAREN
    | 'ACos' LPAREN expr RPAREN
    | 'ATan' LPAREN expr RPAREN
    | 'ATan2' LPAREN expr ',' expr RPAREN
    | 'Degrees' LPAREN expr RPAREN
    | 'Radians' LPAREN expr RPAREN
    | 'Pow' LPAREN expr ',' expr RPAREN
    | 'Sqrt' LPAREN expr RPAREN
    | 'Exp' LPAREN expr RPAREN
    | 'Log' LPAREN expr RPAREN
    | 'Log10' LPAREN expr RPAREN
    | 'Abs' LPAREN expr RPAREN
    | 'Min' LPAREN expr ',' expr RPAREN
    | 'Max' LPAREN expr ',' expr RPAREN
    | 'Clamp' LPAREN expr ',' expr ',' expr RPAREN
    | 'Round' LPAREN expr RPAREN
    | 'Floor' LPAREN expr RPAREN
    | 'Ceil' LPAREN expr RPAREN
    | 'Mod' LPAREN expr ',' expr RPAREN
    | 'Random' LPAREN RPAREN
    | 'RandomInt' LPAREN expr ',' expr RPAREN
    | 'PI' LPAREN RPAREN
    ;

// Lites namespace (grouped light control)
litesCall
    : 'Lites' LPAREN litesArgs RPAREN DOT litesFunction
    ;

litesArgs
    : dottedID (',' dottedID)*
    ;

litesFunction
    : 'Set' LPAREN expr ',' expr ',' expr ',' expr RPAREN
    | 'FadeIn' LPAREN expr ',' expr ',' expr ',' expr ',' expr (',' array)? RPAREN
    | 'FadeOut' LPAREN expr ',' expr ',' expr ',' expr ',' expr (',' array)? RPAREN
    | 'Chase' LPAREN array ',' expr RPAREN
    | 'Pulse' LPAREN expr ',' expr ',' expr ',' expr ',' expr (',' array)? RPAREN
    | 'Blackout' LPAREN RPAREN
    | 'Restore' LPAREN RPAREN
    | 'Toggle' LPAREN RPAREN
    ;

// Array and tuple support for color ranges
array
    : '[' (tuple (',' tuple)*)? ']'
    ;

tuple
    : LPAREN value (',' value)* RPAREN
    ;

// Generic call for other namespaces/devices
genericCall
    : dottedID LPAREN callArgs? RPAREN
    ;

// get(...) and isAvailable(...) calls for any dottedID (method-style)
getCall
    : dottedID DOT 'get' LPAREN callArgs? RPAREN
    ;

isAvailableCall
    : dottedID DOT 'isAvailable' LPAREN RPAREN
    ;

// get(...) and isAvailable(...) as global functions (function-style)
globalGetCall
    : 'get' LPAREN expr RPAREN
    ;

globalIsAvailableCall
    : 'isAvailable' LPAREN expr RPAREN
    ;

callArgs
    : callArg (',' callArg)*
    ;

callArg
    : expr
    | ID ASSIGN expr
    ;

// If/Else statement (non-loop)
ifStatement 
    : IF LPAREN expr RPAREN block (ELSE ifStatement | ELSE block)?
    ;

// Expressions with ternary, arithmetic, array indexing, and .Length()
expr
    : ternaryExpr
    ;

ternaryExpr
    : logicExpr (QUESTION expr COLON expr)?
    ;

logicExpr
    : equalityExpr (OR_OP equalityExpr)*
    ;

equalityExpr
    : relationalExpr (AND_OP relationalExpr)*
    ;

relationalExpr
    : additiveExpr (compOp additiveExpr)*
    ;

compOp
    : EQ
    | NEQ
    | LT
    | GT
    | LE
    | GE
    ;

additiveExpr
    : multiplicativeExpr ( (PLUS | MINUS) multiplicativeExpr )*
    ;

multiplicativeExpr
    : unaryExpr ( (STAR | DIV | MOD) unaryExpr )*
    ;

// Support for prefix ++/--, !, - operators
unaryExpr
    : (NOT_OP | MINUS | INCR | DECR) unaryExpr
    | primaryExpr
    ;

// Support for postfix ++/-- operators
primaryExpr
    : atom ( ('[' expr ']')
           | (DOT 'Length' LPAREN RPAREN)
           | (DOT ID LPAREN callArgs? RPAREN)
           | INCR
           | DECR
           )*
    ;

atom
    : songCall
    | motionCall
    | mathCall
    | colorCall
    | litesCall
    | logCall
    | getCall
    | isAvailableCall
    | globalGetCall
    | globalIsAvailableCall
    | genericCall
    | dottedID
    | value
    | LPAREN expr RPAREN
    ;

// Typed expressions (kept for explicit literal matching, but many functions now use 'expr')
exprFloat
    : FLOAT
    ;

exprInt
    : INT
    ;

exprString
    : STRING
    ;

// Dotted identifier for file names, device paths, and function names, now with array support
dottedID
    : idWithIndex (DOT idWithIndex)*
    ;

idWithIndex
    : ID (LBRACK expr RBRACK)*
    ;

// Values
value
    : FLOAT
    | INT
    | STRING
    | TRUE
    | FALSE
    | NULL
    | ID
    | dottedID
    | array
    | tuple
    ;

// Lexer rules (keywords before ID)
EVENT       : 'event';
VAR         : 'var';
CONST       : 'const';
GROUP       : 'group';
VOID        : 'void';
INT_TYPE    : 'int';
FLOAT_TYPE  : 'float';
STRING_TYPE : 'string';
IF          : 'if';
ELSE        : 'else';
FOR         : 'for';
FOREACH     : 'foreach';
IN          : 'in';
BREAK       : 'break';
CONTINUE    : 'continue';
RETURN      : 'return';
TRY         : 'try';
CATCH       : 'catch';
FINALLY     : 'finally';
TRUE        : 'true';
FALSE       : 'false';
NULL        : 'null';

ENDLOOP     : 'endLoop';

LOG_DOT     : 'Log' '.';
INFO        : 'Info';
ERROR       : 'Error';
WARN        : 'Warn';

EQ          : '==';
ASSIGN      : '=';
NEQ         : '!=';
LT          : '<';
GT          : '>';
LE          : '<=';
GE          : '>=';

SEMI        : ';';
QUESTION    : '?';
COLON       : ':';
DOT         : '.';

PLUS        : '+';
MINUS       : '-';
STAR        : '*';
DIV         : '/';
MOD         : '%';

INCR        : '++';
DECR        : '--';

OR_OP       : '||';
AND_OP      : '&&';
NOT_OP      : '!';

LPAREN      : '(';
RPAREN      : ')';
LBRACK      : '[';
RBRACK      : ']';

ID          : [a-zA-Z_][a-zA-Z0-9_]* ;
FLOAT       : [0-9]+ '.' [0-9]+ ;
INT         : [0-9]+ ;
STRING      : '"' ( '\\' . | ~["\\\r\n] )* '"' ;

WS          : [ \t\r\n]+ -> skip ;
COMMENT     : '//' ~[\r\n]* -> skip ;
