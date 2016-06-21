
BlockInitiator		'{'
BlockTerminator		'}'
StatementTerminator	';'

##CODE-Form

package PATH;	// Namespace
IMPORTS
public $$class/interface/enum$$ {	// one per file, must be equal to Filename
	$$fields$$
	$$ctors$$
	$$dtors$$
	$$get/set$$ // Properties
	$$methods$$
	$$innerClass/Enum$$
}

IMPORTS
import $path$;

##Comments
// Line
/// DocComment
/* Block */
/** DocBlockComment **/	*/

###########
##Class
public class $$NAME$$ {§inc§n$$code$$§dec§n}
Inheritance 	extends $type$ implements $interfaces{0-n}$
	//C# --> : $type{0-or-1, but if then first}$, $interfaces{0-n}$
	super(); // FirstLine in Method/Ctor
##Methods
SCOPE MODS TYPE NAME(ARGS) {...}
public, protected, static
EmptyRetType	void
public static void main(String[] args) {}
public $TYPE$() {} // CTOR
ARGSList	$arg$, $arg$
$arg$		$type$ $name$
All references --> By Reference ("C# = ref")
All literals/values --> By value

##Fields
private static final $$TYPE$$ $$NAME$$ = $value$;

Locals ==> {
$$mod$$ $$type$$ $$name$$;
$$mod$$ $$type$$ $$name$$ = $$value$$;
Chainable (sametype; ',' values allowed)
BUT assignment only requires no ;
	$$name$$ = $$value$$
final
}
BASICS{
String, boolean, int, long
1 --> int
1L --> long
false, true, ""

Can't make ToString on Primitives (not an object)
Wrapper Types are required OR appropriate MethodOverloads

String Concat: "" + ""
#
null
##
Array
	int[] a = {0,1};
	String[] str = new String[10];
	Indexer: arr[i]
	Size => arr.length
	java.util.Arrays;
	--> Arrays.toString(myArr);
		Static Method: for(object i :arr) sb.Append(i.toString())
}
EXCEPTIONS => {
ManagedExceptions = true
	$$MethodSig$$ throws $EXCEPTIONS$,... {$$MethBody$$}

base_Exception = Exception
IOException = java.io.IOException
try {
	
} catch (Exception e) { // 0-n, from specific to general (Exception at Top)
	PRINT ERROR: e.printStackTrace();
	throw; // Rethrows Exception unchanged
	throw e; // Changes Source
	throw new Exception("Reason", e); // Different Exception; Common to add e as Inner 
} catch (Exception e) where $$cond$$ { /// ExceptionFilter:
		// Mostly OR-Chain of "e is FileNotFoundException"
		// or e.XY == Blarg
} catch (ServletException | IOException e) { /// Multi Catch

} finally { }
	
###
}
StatementExpr	$$;
	VarDecl, MethodCall, Assignment, return

Operations {
## Operations ##
$$ == $$	$$.equals($$)
$$ && $$
!$$
$$ = $$
$$ += $$
TYPE.STATICMETHOD()
LOCAL.METHOD()
new CTOR()

import $$path$$;
	IO --> java.io.*;
while($$cond$$) {$$code$$}
if($$cond$$){$$code$$}
for(int i=0; i< arr.length; i++){}
for(int i : arr){}
return $$;

###
AccessorMethod/Getter
public $$Type$$ get$$Field$$() { return $$field$$; }
public void set$$Field$$($$Type$$ $$field$$) {
	this.$$field$$ = $$field$$; }
}
Struct ==> public class {
	private Fields = defaults;
	public Ctor() {}
	public Type Accessor(){}
	public void Manipulator(Field){}
	
}
Lambda ==> class x {
	void method(){
		int[] a = { 1,2,3,4};
		iMethod meth = (arr) -> {
			int summe = 0;
			for (int i : arr) summe += i;
			return summe;
		}
		meth.sum(a);
	}
	interface iMethod {int sum(int[] arr);}
}
Reflection{
###
public static <T> T method(T t){
	return (T) otherMethod(t);
}
void method(Class<T> interClass, Class<? extends T> implClass)

// Type t = typeof(T); // GenericParam
// Type t = obj.getType();
Class<?> clazz = obj.getClass();
Field field : clazz.getDeclaredFields()
Class<T>.newInstance() --> Invoke Constructor
Class<T>.getName()
	isInterface()
	getConstructor()


}

Specifics {

FileReader (import _IO_)
	final InputStream iStream = new FileInputStream("PATH");
FileWriter {
	PrintWriter out;
	final OutputStream iStream = new FileOutputStream(new File("F:/HALP.txt"));
	try{
		out = new PrintWriter(iStream);
		out.println();
	} finally {}
}
##
StringBuilder (import none)
	StringBuilder sb = new StringBuilder();
	sb.append().toString()

class XMLParser {
	import java.io.FileInputStream;
	import java.io.InputStream;

	import javax.xml.stream.XMLInputFactory;
	import javax.xml.stream.XMLStreamConstants;
	import javax.xml.stream.XMLStreamReader;
	
	void Method(){
		XMLInputFactory factory = XMLInputFactory.newInstance();
		final String FILE_INPUT = "...";//"../applications/<<WARFILE>>/data/Bestellung.xml";
		
		final InputStream iStream = new FileInputStream(FILE_INPUT);
		XMLStreamReader parser = factory.createXMLStreamReader(iStream);
		while(parser.hasNext()){
			int event = parser.next();
			if (event == XMLStreamConstants.START_ELEMENT) {
				if (parser.getLocalName().equals("Positionsnummer")) {
					String elem = parser.getElementText();
					String elem = parser.getAttributeValue(0);
					if (elem.equals(position)) {
						hit = true;
						found = true;
						sb.append("Positionsnummer: " + elem + ", ");
					}
				}
			}
		}
	}
}


}
__DEV{
####
	interface iMethod {int sum(int[] arr);}
	iMethod meth = (arr) -> 0;
	meth.sum({0,1});
### Main difference: Interface is a typedef, delegate is a type
	// delegate int iMethod(int[] arr);
	// iMethod sum = (arr) => 0;
	// sum(new[]{0,1});
###
	§newDelegate("iMethod", eDType.Int32, new[,]{{new DType(eDType.Int32, isArray: true), "arr"}})
		// for C#
			§newMember(eDType.Delegate, "iMethod", eDType.Int32, new[,]{{new DType(eDType.Int32, isArray: true), "arr"}})
		// for Java
			§newInner(eType.Interface, "iMethod")
			§addMethod("iMethod", "invoke", eDType.Int32, new[,]{{new DType(eDType.Int32, isArray: true), "arr"}})
	// §newInner  := eType, name // for InnerTypes; Name is actually registered to add members to it.
	// §newMember := eType, name, Type, Value // in case of Methods, Argument list
	// §addMethod := AddToType, MethodName, returnType, Arguments
		This will also take care of C-like SignatureDefinitions in CodeHead (if option is set)
	##
	§locDef("iMethod", "sum") // either eDType or a string
	sum = §lambda(new[]{"arr"}) // will start nested. Also register as being required/allowed to invoked
	return 0;
	§endLambda
	##
	§invoke("sum", new[]{0,1}); // Should crash if not found in "invokeable"-List
####
## C# only
Nullables:
	Struct: Add '?' to Type (makes it Nullable<T>)
		Either (x == null && x.Value.Y == ...)
	OR
		x?.Y == ...
}