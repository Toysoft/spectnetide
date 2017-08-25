grammar Z80Asm;

/*
 * Parser Rules
 */

compileUnit
	:	(asmline NEWLINE?)* EOF
	;

asmline
	:	label? (pragma | instruction) | NEWLINE
	;

label
	:	IDENTIFIER
	;

pragma
	:	orgPragma
	|	entPragma
	|	dispPragma
	|	equPragma
	|	defbPrag
	|	defwPrag
	|	defmPrag
	;

orgPragma
	:	ORGPRAG expr
	;

entPragma
	:	ENTPRAG expr
	;

dispPragma
	:	DISPRAG expr
	;

equPragma
	:	EQUPRAG expr
	;

defbPrag
	:	DBPRAG expr (',' expr)*
	;

defwPrag
	:	DWPRAG expr (',' expr)*
	;

defmPrag
	:	DMPRAG STRING
	;

instruction
	:	trivialInstruction
	|	loadInstruction
	|	incrementInstruction
	|	decrementInstruction
	;

trivialInstruction
	:	NOP
	|	RLCA
	|	RRCA
	|	RLA
	|	RRA
	|	DAA
	|	CPL
	|	SCF
	|	CCF
	|	RET
	|	EXX
	|	DI
	|	EI
	|	NEG
	|	RETN
	|	RETI
	|	RLD
	|	RRD
	|	LDI
	|	CPI
	|	INI
	|	OUTI
	|	LDD
	|	CPD
	|	IND
	|	OUTD
	|	LDIR
	|	CPIR
	|	INIR
	|	OTIR
	|	LDDR
	|	CPDR
	|	INDR
	|	OTDR
	;

// --- Load instruction
loadInstruction
	:	load8BitRegInstruction
	|	loadRegWithValueInstruction
	|	loadRegAddrWith8BitRegInstruction
	|	load8BitRegFromRegAddrInstruction
	|	loadMemAddrWithRegInstruction
	|	loadRegFromMemAddrInstruction
	;

load8BitRegInstruction
	:	LD (REG8 | HLIND) ',' (REG8 | HLIND)
	;

loadRegWithValueInstruction
	:	LD (REG8 | HLIND | REG16 | IDXREG) ',' expr
	;

loadRegAddrWith8BitRegInstruction
	:	LD '(' (REG16 | indexedAddr) ')' ',' REG8
	;

load8BitRegFromRegAddrInstruction
	:	LD REG8 ',' '(' (REG16 | indexedAddr) ')'
	;

loadMemAddrWithRegInstruction
	:	LD '(' expr ')' ',' (REG8 | REG16 | IDXREG)
	;

loadRegFromMemAddrInstruction
	:	LD (REG8 | REG16 | IDXREG) ',' '(' expr ')'
	;

indexedAddr
	:	IDXREG (('+' | '-') expr)?
	;

// --- Increment and decrement
incrementInstruction
	:	INC (REG8 | HLIND | REG16 | IDXREG | ( '(' indexedAddr ')' ))
	;

decrementInstruction
	:	DEC (REG8 | HLIND | REG16 | IDXREG | ( '(' indexedAddr ')' ))
	;

// --- Expressions
expr
	: xorExpr ('|' xorExpr)*
	;

xorExpr
	: andExpr ('^' andExpr)*
	;

andExpr
	: shiftExpr ('&' shiftExpr)*
	;

shiftExpr
	: addExpr (('<<' | '>>' ) addExpr)*
	;

addExpr
	: multExpr (('+' | '-' ) multExpr)*
	;

multExpr
	: unaryExpr (('*' | '/' | '%') unaryExpr)*
	;

unaryExpr
	: '+' unaryExpr
	| '-' unaryExpr
	| '[' expr ']'
	| literalExpr
	| symbolExpr
	;

literalExpr
	: DECNUM 
	| HEXNUM 
	| CHAR
	| '$'
	;

symbolExpr
	: IDENTIFIER
	;

/*
 * Lexer Rules
 */

COMMENT
	:	';' ~('\r' | '\n')* -> channel(HIDDEN)
	;

WS
	:	(' ' | '\t') -> channel(HIDDEN)
	;

NEWLINE
	:	('\r'? '\n' | '\r')+
	;

// --- Trivial instruction tokens

NOP		: 'nop' | 'NOP';
RLCA	: 'rlca' | 'RLCA';
RRCA	: 'rrca' | 'RRCA';
RLA		: 'rla' | 'RLA';
RRA		: 'rra' | 'RRA';
DAA		: 'daa' | 'DAA';
CPL		: 'cpl' | 'CPL';
SCF		: 'scf' | 'SCF';
CCF		: 'ccf' | 'CCF';
RET		: 'ret' | 'RET';
EXX		: 'exx' | 'EXX';
DI		: 'di' | 'DI';
EI		: 'ei' | 'EI';
NEG		: 'neg' | 'NEG';
RETN	: 'retn' | 'RETN';
RETI	: 'reti' | 'RETI';
RLD		: 'rld' | 'RLD';
RRD		: 'rrd' | 'RRD';
LDI		: 'ldi'	| 'LDI';
CPI		: 'cpi' | 'CPI';
INI		: 'ini' | 'INI';
OUTI	: 'outi' | 'OUTI';
LDD		: 'ldd'	| 'LDD';
CPD		: 'cpd' | 'CPD';
IND		: 'ind' | 'IND';
OUTD	: 'outd' | 'OUTD';
LDIR	: 'ldir'| 'LDIR';
CPIR	: 'cpir' | 'CPIR';
INIR	: 'inir' | 'INIR';
OTIR	: 'otir' | 'OTIR';
LDDR	: 'lddr'| 'LDDR';
CPDR	: 'cpdr' | 'CPDR';
INDR	: 'indr' | 'INDR';
OTDR	: 'otdr' | 'OTDR';

// --- Other instruction tokens
LD		: 'ld' | 'LD';
INC		: 'inc' | 'INC';
DEC		: 'dec' | 'DEC';

// --- Pragma tokens
ORGPRAG	: '.org' | '.ORG' | 'org' | 'ORG';
ENTPRAG	: '.ent' | '.ENT' | 'ent' | 'ENT';
EQUPRAG	: '.equ' | '.EQU' | 'equ' | 'EQU';
DISPRAG	: '.disp' | '.DISP' | 'disp' | 'DISP';
DBPRAG	: '.defb' | '.DEFB' | 'defb' | 'DEFB';
DWPRAG	: '.defw' | '.DEFW' | 'defw' | 'DEFW';
DMPRAG	: '.defm' | '.DEFM' | 'defm' | 'DEFM';

// --- 8-bit registers
REG8	: 'a' | 'A' | 'b' | 'B' | 'c' | 'C' | 'd' | 'D' | 'e' | 'E'
		| 'h' | 'H' | 'l' | 'L' | 'r' | 'R' | 'i' | 'I'
		| 'xl' | 'XL' | 'xh' | 'XH' | 'yl' | 'YL' | 'yh' | 'YH';

HLIND	: '(' WS* ('hl' | 'HL') WS* ')';

REG16	: 'af' | 'AF' | 'bc' | 'BC' | 'de' | 'DE' | 'hl' | 'HL' | 'sp' | 'SP' ;

IDXREG	: 'ix' | 'IX' | 'iy' | 'IY';

DECNUM	: DIGIT DIGIT? DIGIT? DIGIT? DIGIT?;
DIGIT	: '0'..'9';

HEXNUM	: '#' HDIGIT HDIGIT? HDIGIT? HDIGIT?
		| HDIGIT HDIGIT? HDIGIT? HDIGIT? ('H' | 'h');

HDIGIT	: '0'..'9' | 'a'..'f' | 'A'..'F';

CHAR	: '"' ( '\"' | . ) '"';

STRING	: '"' ( '\"' | . )* '"';

IDENTIFIER
	:	IDSTART IDCONT*
	;

IDSTART
	:	'_' | 'A'..'Z' | 'a'..'z'
	;

IDCONT
	:	'_' | '0'..'9' | 'A'..'Z' | 'a'..'z'
	;
