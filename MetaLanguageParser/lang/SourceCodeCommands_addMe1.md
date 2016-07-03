§vardecl($$type$$ $$name$$);
§vardecl($$type$$ $$name$$) = $$value::readExpression$$;
--> Added to current Method
§vardecl({String, name1, name2}, {Int32, int1, int2});
	MultiDeclaration (without assignment thou).
§assign($$name$$)($$expr::readExpression$$);
##
§addMethod(§main)      /// Adds a EntryMethod according to "/myLang/§main.{suffix}"
//§addMethod()(myMethod) /// Adds a method called "myMethod" with no modifiers or Parameters, + ReturnType = void
§endMethod             // Ends the current MethodBody and adds it to the enclosed Type
##
§addType(§main)        // Adds a simple "public class Programm" according to "/myLang/§mainType.{suffix}"
§endType               // Ends the current TypeDeclaration and adds it to the list of declared types
##
Added an configFile

##
Operations:
Either a direct Primitive
	Boolean: true, false
	Any number
	--> Allowed are DecimalSeperator, ThousandSeperator (Has to be tested)
	--> UnaryPlus/Minus and (-0) might be tokenized apart, but both are valid numbers
	--> Currency and HexPrefix are available as well, but have to figured out if required.
	"Text" ?
	'Chars' ?
Anything else is interpreted as "Variable"
Or an conditional / arithmetic term
	(+ {op1} {op2})
