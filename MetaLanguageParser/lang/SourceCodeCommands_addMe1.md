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