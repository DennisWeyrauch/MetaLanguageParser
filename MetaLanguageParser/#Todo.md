## Todo ##
* ~~Supply Codefile via arg as well~~
* ~~(!!) Add proper method/class structure (at least Main/Programm) to allow executing the code~~
* ~~(!!!) Typing model~~
* (!) Gradually build Memberstructure (see #dev02#)
  *	Done: Local, Main
* Put the Tokenizer after the reading of Operators, and somehow include them in the splitting
* ~~Don't forget to collect the files via ResxExtension now.~~
* In the check of if main Config exists, also check (throw on flag) if operator, §progam and §main exist
* Make more stuff work with Local + Method
* ~~Clean up the indentation mess~~
* ~~Add Command Explaination to them~~
  * ~~ - Say that thing about _vardecl, and that removing §define always splits up the DeclarationAssignments~~
* ~~(!!) Read in Datatypes~~
  * --> Maybe even custom ones?
* (!!) Read in Modifiers
  * --> Unlikely that custom ones are included
* Move True/False Definition into _types.txt
* 03-Arrays.txt
  * §array, §get, §set
* 04-Casting.txt
  * §cast, §addCast
* 2016-07-14
  * Fix LineBreakProblems in VB.net
  * ... and these two empty newlines still bug me
  * Read in meta/types to disallow undefined types
  * Enable more LiteralDeco
  * Add §comment to Docu
* 2016-07-15
  * Add MetaAdapter Class (Details see TextFile1.txt)
  * Add new Methods from Switch.cs


### Commands
(#) CodeCommands
(#) DefParseCommands
 -- §checkOption --> see devFile
(#) DestParseCommands
 -- §forEach // For SwitchCases
 -- §addImport
 -- §addMethod(){ // Check is missing
	// Checks if a method with this signature already exists or generates if not, and inserts a MethodCall at the current pos.
	Example: toCharArray() has to be defined in Javascript
}
