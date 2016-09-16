§addType(x y z myType KEYWORD)

Maybe here destinction between "HasTypes -- Yes and No" ?
	Maybe "You declared types, but want to use a Procedural language. Continue?" // add (s) to save as default in config
	Could then search for _writeType.{lang} in MetaCode/ProcType

Tokenize at ' '
If found in "Modifiers", add with MetaVal, continue with next
If not, take next as Name
#Option Generics true/false
	If false: Write("{Name+Generics}: Not supported in {language}") and terminate OR add InnerType "T extends Object" ? (GenName extends GenFilter)
Next is:
	Either a Keyword OR some kind of seperator (Braces maybe)
		FROM keyword?
However made, Extension (if found) follows
	#Option MultiClass true/false
	If false: Write("This language does not allow multiple inheritance") followed by Name + Parents
		Disallow? Ask to skip? Ask to ignore rest classes (maybe into comment behind)? 
	In case of Csharp, it does not differ, and just chains all, only assuming first to be Parent
Likewise, Impl follows, with interface types
	#Option as well?

Set currentType to this, which allows §member Hooks. (Field, Ctor, CCtor, Dtor, Prop)
####

§addMethod/Field/Prop likewise
Modifiers, until not found (in appropriate category)
Then try to match Type
	Methods allow Void, else if type not found insert _INVALID_(retType)
Next is Name
#Fields:
	END or Assign
#Prop
	either default --> get/set of _myName (Field), or CSharp, {get; set;}
	or get {...}, set {...} // SetArg keyword -> Value
	Java will do them as Methdos (get_Name and set_Name(TYPE value))
#Methods
	Generics, as above
	() Braces, signals ArgStart, OR Newline + Indent
public void myMethod(
	in Int32 arg1, // in == Normal In Variable. Uses Default of Lang, but usually "ByValue"
	out Int32 arg2, // out == Like C#-own out
	inout Int32 arg3, // Ref parameter == forces "ByReference" (like &a instead of only a in C)
	.. arg4 = "", // Optional == Adding default Value. Option / Fallback Solution required (in Form of Assign Value first thing in method)
	vargs ... arg5, // Variable Argument, 