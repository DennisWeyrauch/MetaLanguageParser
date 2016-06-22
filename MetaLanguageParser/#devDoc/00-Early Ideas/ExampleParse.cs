/// Steps to do:

bool doDebug = false;
public delegate int FuncDel(ref ExeBuilder eb, ref int pos);
public static Dictionary<string, FuncDel> kw;

public Parser(bool debug = false){
	doDebug = debug;
	kw = new Dictionary<string, FuncDel>();
}

public void execute(string file){
	// Create a Tokenizer
	var toker = new Tokenizer();
	// Generate a ListWalker
	ListWalker list = toker.execute(file);
	// Declare some handy locals
	int len = list.Count;
	string elem;
	FuncDel func; // Holds the functionPointer
	ExeBuilder eb = null;
	try {
		eb = new ExeBuilder(list);
		while (list.Index < len){
			try{
				elem = list.getCurrent();
				if (kw.TryGetValue(elem, out func)) func.Invoke(ref eb, ref list.Index);
				else throw new InvalidSyntaxException("PARSER", elem, list.Index);
			} catch (InvalidSyntaxException) {}
		}
	} finally {
		eb?.finalizeCode();//
		// That rolled out to 
		...
	}
}