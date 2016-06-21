# MetaLanguageParser

This project came out of lazyness to translate codepuzzles from www.codingame.com into multiple languages.
So, it will be somewhat restricted to what is required to do there (at first), but I might expand it beyond that.

The ProjectFolder "Common" is just a collection of general purpose code I wrote together during earlier projects.
It is linked with the main Project in a way that it generates a dll into the main's execution directory, therefore "Common" is not required for development (but as there is still no DocXml, it's pretty useless without the sources...)

########
## Code
Write any code you like.
The first token must be identical with the filename (else it can't be found (duh))

########
## Keywords
Write ParserCalls in the form:

	$$name::methodHandle$$
with

	name := how to access contents in destFiles (with $$name$$)
	methodHandle := which method to call (see below)
Escape $$ if required. (This is still unimplemented!)

### Available Methods:
* readConditional
	* Attempts to parse an conditional expression that resolves to a boolean value.
* readCode
	* Read generic code body
* readAnyCode
	* TestCommand: Will skip any code until the closing brace is found (assuming they're correctly balanced)
* readParameterList
	* For Method Invokations.

### Example
#### lang//meta/§write.txt (MetaDefinition)
	§write($$args::readParameterList$$)
#### lang//csharp/§write.txt (MetaDestination)
	System.Console.Write($$args$$)

########
## Operators
Define Operators in /meta/_op_Bool.txt and /meta/_op_Arith.txt, which are for Boolean resp. Arithmetic Operations.

* §§Unary and §§Binary define one/two Operators
* Form: IDSTRING -> Tab -> MetaOpSymbol

Map them in /myLang/_operators.txt (mixed)
* IDSTRING -> Tab -> FormatString to write

### Example
	MetaFile
		Negate	!
	LangFile
		Negate	!{0}

Lines starting with # are skipped and can be used for comments.

In the Codefile, write Operations in the following form: (similar to functional languages)
* For Unary:
	* operator Arg1
* For Binary:
	* operator Arg1 Arg2

Nested Operations are done via ()
Example:

	if ((i < 15) & (i > 0))

Becomes

	if (& (< i 15) (> i 0))

