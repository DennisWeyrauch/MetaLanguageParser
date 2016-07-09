This file should documentate ParserHooks useable in the codeFile or in the mDest Files.


# Definition Files #
$$varName::parserHook$$

# CodeFile #
## Types ##
This will create a new TypeDeclaration. 
* If a Module has been declared, it will be set as this types Module.
* If we are currently inside an Type allowing nested Types, it's module + Typename will be set as this types Module, and it will be set as the currentType

Each Type can contain different member types in any order, but they will be emitted grouped in order of declaration according to "_newType.txt"
* ~~Fields into $$fields$$~~
* ~~Constructors into $$ctor$$~~
* ~~TypeConstructors into $$cctor$$~~
* Methods into $$methods$$
  * If found, EntryMethods into $$entryMethod$$
  * ~~If the option "InlineMainCode" exists, then $$entryMethodCode$$ is available~~
* ~~Nested Types into $$types$$~~

Allowed commands are:
* §addType(§main)
  * Adds a simple "public class Programm" according to "/myLang/§mainType.{suffix}"
  * The same KeyHolders as for "_newType.txt" have to be applied
* §endType
  * Will close off the currentType, or return to the enclosed type in case of a nested one.
  * ~~Will throw an InvalidOperationException if no Type was openend previously.~~

## Methods ##
Allowed commands are:
* §addMethod(§main)
  * Adds a EntryMethod according to "/myLang/§main.{suffix}"
* §endMethod
  * Ends the current MethodBody and adds it to the enclosed Type
  * ~~Will throw an InvalidOperationException if no Method was openend previously.~~
    * ~~If the option "InlineMainCode" exists, then they will be added to the inlineType.~~
  * ~~If CodingParadigma is set to "Procedural", then ...~~


## Variables and Assignment ##
Declare locals for the current method. To simplyfy portability, there will be no scope.
All variables will be emited in order of their declaration.
Depending on the definition inside "_vardecl.txt", either together or seperated from the assignment.

Forms:
* §vardecl($$type$$ $$name$$);
* §vardecl($$type$$ $$name$$) = $$value::readExpression$$;
* §vardecl(§TypeDecl1§, §TypeDecl2§, ...);
  * §TypeDecl1§ := {$$Type$$, $$Name1$$, $$Name2$$, ...}
* §assign($$name$$)($$expr::readExpression$$);


# Destination Files #
These Files contain the code fragments to insert for the keywords found in the CodeFile.
To use an saved ParserExpression retrieved in the Definition Files, use $$KEY$$ where desired.
Some special files have predefined KeyHolders, but the below Commands are applied likewise.

Allowed commands are:
* NewLine -- Just make a linebreak (Multiple linebreaks without code in between will be truncated)
  * Use §n to force linebreaks in Result (will then use the System's newline culture)
  * Best Result when used NOT on own lines, but on the end of codelines 
* §inc -- Increment Indent (' ' as visual seperation from code is allowed and will be removed subsequently)
* §dec -- Decrement Indent (Likewise)
* §retract -- Retract SourcePos (Will be done right before printing Code to File)
  * §retract(6) --> Delete previous 6 characters. 
  * Example: VB.net --> "End If§retract(6)Else" --> "Else"
  * Obviously should only be used at the start of files to be useful.
* ~~§addMethod($$type$$, name)$$code$$§endMethod~~
  * Will add this method as is to the given Type. $$code$$ is expected to be a functional methodBody excluding the signature
  * ~~If the option "InlineMainCode" exists, the inlineType "null" type will be allowed.~~
  * ~~Multiple AddCalls might be allowed in one File~~
