########
## Code

Write any code you like:
	The first token must be identical with the filename (else it can't be found (duh))

########
## Keywords
Write ParserCalls in the form
	$$name::methodHandle$$
	Escape $$ if required. (This is still unimplemented)
with
	name := how to access contents in destFiles (with $$name$$)
	methodHandle := which method to call
## Available Methods:
readConditional
	Attempts to parse an conditional expression that resolves to a boolean value.
readCode
	Read generic code body
readAnyCode
	TestCommand: Will skip any code until the closing brace is found (assuming they're correctly balanced)
readParameterList
	For Method Invokations.
readParameterList
	???
## Example
# /meta/§write.txt
§write($$args::readParameterList$$)
# /csharp/§write.txt
System.Console.Write($$args$$)

########
## Operators
Define Operators in /meta/_op_Bool.txt and /meta/_op_Arith.txt, which are for Boolean resp. Arithmetic Operations.
 -- §§Unary and §§Binary define one/two Operators
 -- Form: IDSTRING -> Tab -> MetaOpSymbol
Map them in /myLang/_operators.txt (mixed)
 -- IDSTRING -> Tab -> FormatString to write
    Example
	MetaFile
		Negate	!
	LangFile
		Negate	!{0}
Lines starting with # are skipped and can be used for comments.
In the Codefile, write Operations in the following form:
    For Unary:
	operator Arg1
    For Binary:
	operator Arg1 Arg2
Nested Operations are done via ()
    Example:
	if ((i < 15) & (i > 0))
    Becomes
	if (& (< i 15) (> i 0))
