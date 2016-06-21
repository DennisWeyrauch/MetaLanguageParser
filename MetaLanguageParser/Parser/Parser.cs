using System; // Console, DateTime
using System.Collections.Generic; // List<T>
using System.Globalization; // DateTimeInfo
using System.IO;   // StreamWriter, IOException, File, FileStream, enum FileMode, Directory
using System.Text; // StringBuilder
using System.Text.RegularExpressions; // MatchEvaluator, Match, enum RegexOptions, Regex
using Common;
using static MetaLanguageParser.Resources.ResourceReader;
//using static MetaLanguageParser.Resources.LangConfig;

namespace MetaLanguageParser
{
    using MetaCode;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Resources;
    using System.Text;
    /// <summary>
    /// Abstract base class that does most of the work
    /// </summary>
    public class Parser
    {
        internal static bool _running = false;
        /// <summary>
        /// Read-only property to indicate whether or not the program already runs.
        /// </summary>
        public static bool IsRunning => _running;
        static bool doDebug = false;
        public static bool getDebug => doDebug;
        static string log = "logs\\__ParseTest.log";
        static string logStack = "logs\\__ParseStack.log";
        static string basePath = Resources.ResxFiles.basePath;
        //string input = "";
        Tokenizer toker;
        public static bool scopeChanged = false;
        public static Dictionary<string,FuncDel> kwDict;
        public static List<string> kw;
        public static int depth = 0;
        public static Parser _instance;
        public static Parser getInstance => _instance;

        public Parser(bool debug = false)
        {
            //doDebug = Program.doDebug; // static set via main
            //StringBuilder sb = new StringBuilder().Append(File.ReadAllText(files[0]));
            doDebug = debug;
            toker = new MetaTokenizer();
            _instance = this;
            kw = new List<string>();
        }


        /// <summary>Main trigger method to start building</summary>
        public void execute(string file, string language) // 111
        {
            if (_running) {
                Console.WriteLine("Another instance already exists!");
                return;
            }
            _running = true;
            int len; // Reserving Slots for locals.
            var list = toker.execute(file); // Slot 3/4
            ExeBuilder eb = null;

            StringWriter output = new StringWriter();
#warning CUSTOM:: Indent
            System.CodeDom.Compiler.IndentedTextWriter writer = null;
            try {

                eb = new ExeBuilder(list, language); // Slot 4/4
                Resources.ResourceReader.readConfiguration(language);
                //Resources.MyResourceReader.readResx(language);
                writer = new System.CodeDom.Compiler.IndentedTextWriter(output, __INDENT);


                kw.AddRange(Directory.EnumerateFiles(Resources.ResxFiles.getMetaPath()).Select(Path.GetFileNameWithoutExtension));
                //kwDict = new ParserStorage().fillLists();
                kw.Remove(language);
                len = list.Count;

                while (list.Index < len) {
                    var str = execRun(ref eb, ref list.Index);
                    writer.Write(str);
                   // if(list.isClosure)
                }
            } catch (Exception e)
                    when (e is InvalidSyntaxException || e is InvalidOperationException) { 
					#warning INFO:: Added Catch to printErrors on highest layer
                    list.printError();
					hasThrown = true;
			} finally {
                File.WriteAllText($"Results.{__FILESUFFIX}", output.ToString());
				if (hasThrown) list.printError(finalize: true);
                writer?.Dispose();
                output?.Dispose();
            }
            _running = false;
        }






        bool hasThrown = false;
        int depthCnt = 0;
        public string execRun(ref ExeBuilder eb, ref int pos)
        {
            if (!_running) throw new InvalidOperationException("Calling recursive Parser Method without init!");
            var list = eb.list;
            string elem = "";
            depthCnt++;

            ICode ce = new CodeExample();
            var output = new StringWriter();
            var writer = new System.CodeDom.Compiler.IndentedTextWriter(output, "\t");
            
            while (true) {
                try {
                    elem = list.getCurrent();
                    if (kw.Contains(elem)) writer.Write(ce.parse(ref eb, ref pos));
                    /**///else if (Type.GetType(elem, false) != null) kw["void"].Invoke(ref eb, ref list.Index);
                    else if (list.isAtEnd()) {
                        //if (list.isClosure()) list.Index++;
                        elem = output.ToString();
                        break;
                    }//*/
                    else throw new InvalidSyntaxException("PARSER", elem, pos);
                } catch (Exception e)
                    when (e is InvalidSyntaxException || e is InvalidOperationException) {
                    if (doDebug) throw;
                    if (e.Message.Contains("PARSER")) throw;
                    list.printError();
                    hasThrown = true;
                    writer.Write("<<<ERROR>>>");
                    list.skipBalanced('{', '}');

                    //if (e.Message.Contains("<Invalid>")) { int debugHook = 0; }
                    //CatchHandler(ref eb, ref eb.list.Index, e);
                }/* catch (Exception e) { }//*/
            }
            depthCnt--;
            //if(hasThrown && depthCnt == 0) list.printError(finalize: true);
            writer.Dispose();
            output.Dispose();
            return elem;
        }

        public string execStatement(ref ExeBuilder eb, ref int pos) // , bool isStatement = false
        {
            if (!_running) throw new InvalidOperationException("Calling recursive Parser Method without init!");
            var list = eb.list;
            string elem = "";
            //depthCnt++;
            FuncDel func;

            ICode ce = new CodeExample();
			#warning __Is this even required? It's just one simple string, so no indent is required anyway
            //var output = new StringWriter();
            //var writer = new System.CodeDom.Compiler.IndentedTextWriter(output, "\t");
			var str = "";
            //while (true) {
                try {
                    elem = list.getCurrent();
					#warning INFO:: Added Dictionary as else; add §varDecl, §lambda, etc. with different methods
                    if (kw.Contains(elem)) str = ce.parse(ref eb, ref pos);
                    else if (list.isAtEnd()) {}/*// this is cheaper/faster than TryGetValue
                        //elem = output.ToString();
                        //break; // out cause no loop
                    }//*/
					#warning INFO:: Add something for Assign/MethodCall to use X=y and X.Y instead of §assign and §call
					//else if(varDict...) str = ... // Assignement/MethodCall
					else if(kwDict.TryGetValue(elem, out func)) str = func.Invoke(ref eb, ref pos);
                    else throw new InvalidSyntaxException("PARSER", elem, pos);
                } catch (Exception e)
                    when (e is InvalidSyntaxException || e is InvalidOperationException) {
                    if (doDebug) throw;
                    if (e.Message.Contains("PARSER")) throw;
                    list.printError();
                    hasThrown = true;
                    str = "<<<ERROR>>>";
#warning INFO:: Think of a solution
                    list.skipUntil(';');
                    
                }
                 //depthCnt--;
            if (list.isCurrent(';')) {
#warning CUSTOM:: Statement Terminator
                str += ";";
                pos++;
            }
            //if (hasThrown && depthCnt == 0) list.printError(finalize: true);
            /*elem = output.ToString();
            writer.Dispose();
            output.Dispose();//*/
            return str;
        }

#if false
        public string execRun_old(ref ExeBuilder eb, ref int pos)
        {
            if (!_running) throw new InvalidOperationException("Calling recursive Parser Method without init!");
            var list = eb.list;
            string elem = "";
            FuncDel func;
            var output = new StringWriter();
            var writer = new System.CodeDom.Compiler.IndentedTextWriter(output, "\t");
            while (true) {
                try {
                    elem = list.getCurrent();
                    if (kwDict.TryGetValue(elem, out func)) writer.Write(func.Invoke(ref eb, ref pos));
                    /**///else if (Type.GetType(elem, false) != null) kw["void"].Invoke(ref eb, ref list.Index);
                    else if (list.isAtEnd()) {
                        if (list.isClosure()) list.Index++;
                        elem = output.ToString();
                        break;
                    }//*/
                    else throw new InvalidSyntaxException("PARSER", elem, pos);
                } catch (Exception e)
                    when (e is InvalidSyntaxException || e is InvalidOperationException) {
                    if (doDebug) throw;
                    if (e.Message.Contains("PARSER")) throw;
                    list.printError();
                    while (list.whileNot(ref elem, '}')) ;

                    //if (e.Message.Contains("<Invalid>")) { int debugHook = 0; }
                    //CatchHandler(ref eb, ref eb.list.Index, e);
                }/* catch (Exception e) { }//*/
            }
            writer.Dispose();
            output.Dispose();
            return elem;
        }
        /// <summary>
        /// This will assume the file only contains code that makes up a Methodbody, and wrapps the required structures around.
        /// </summary>
        public void executeCodeOnly() // 111
        {
            if (_running) {
                Console.WriteLine("Another instance already exists!");
                return;
            }
            _running = true;
            int len; // Reserving Slots for locals.
            var list = toker[0].execute(); // Slot 3/4
            string elem; ExeBuilder eb; ILGenerator ilg;
            ps = new ParserStorage(); ps.fillLists();
            eb = new ExeBuilder("CodeDummy", "CodeTest", out ilg);

            //new ParserStorage().fillLists();
            len = list.Count;
            Keyword.del func;
            "".WriteText(log);
            "".WriteText(logStack);
            while (list.Index < len) {// pos++) {
                try {
                    elem = list.getCurrent(); /// #### Testing around to catch ; properly.
                    if (kw.TryGetValue(elem, out func)) func.Invoke(ref eb, ref list.Index);
                    else if (Type.GetType(elem, false) != null) kw["void"].Invoke(ref eb, ref list.Index);
                    else if (elem.EqualsChar('}')) {
                        list.Index++;
                        eb.popScope(); continue;
                        /*} else if (ps._listBinary.Contains(elem = tokens[pos + 1])) {
                            pos++; continue; //*/// This is not allowed here anyway, so away with it.
                    } else throw new InvalidSyntaxException("PARSER", elem, list.Index);
                } catch (Exception e) // This is one hell of a catch block
                    when (e is InvalidSyntaxException || e is InvalidOperationException) {
                    if (doDebug) throw;
                    if (e.Message.Contains("PARSER")) throw;
                    if (e.Message.Contains("<Invalid>")) {
                        int debugHook = 0;
                    }
                    CatchHandler(ref eb, ref eb.list.Index, e);
                }/* catch (Exception e) { }//*/
            }
            // The ASM sealing is missing.


            //Console.Write(eb.ToString());
            /*/
            eb.ToString(true).WriteText("dictLog\\Demo_log.cs");/*/
            eb.ToFile("dictLog\\Demo_log.cs", false, true); //*/
            _running = false;
        }

        internal static void CatchHandler(ref ExeBuilder eb, ref int pos, Exception e){
            string msg = e.Message;
            var list = eb.list;
            if (!msg.Contains("Pos:")) msg = $"{msg} (Pos: {pos})";

            Console.WriteLine(msg);
            msg.AppendLine(log);
            $"\r\n{msg}\r\n{e.StackTrace}".AppendLine(logStack);
            if (msg.Contains("} expected")) {
                //e.Message.AppendLine(log);
                eb.popScope();
                pos++;
                return;
            } else if (msg.Contains("; expected")) return;
            if (e.StackTrace.Contains("CreateType")) {
                eb.forcePop();
                $"<<Dropped Invalid Type>>".AppendLine(logStack);
                pos++;
                return;
            }

            int old = pos;
            while (list.getNext().IndexOfAny(";{}") == -1) ;
            $"<<{pos - old} skipped>>".AppendLine(logStack);

            switch (list.getCurrentAsChar()) {
                case ';':
                    if (eb._scope == Scope.AbstractMethod) eb.popScope(); break;
                case '{':
                    var s = eb._scope;
                    if (s == Scope.AbstractMethod) break;//This shouldn't be possible anyway, but well...
                    else if ((s & Scope.Method) == Scope.Method) {
                        if (s != Scope.Method) eb.pushScope(Scope.Method);
                        eb.pushScope(Scope.MethodBody);
                    } else if ((s & Scope.MethodBody) == Scope.MethodBody) eb.pushScope(Scope.Nested);
                    else if ((s & Scope.Type) == Scope.Type || s < (Scope)3) {
                        if (s != Scope.Type) eb.pushScope(Scope.Type);
                        eb.addType($"<invalid_pos{pos}>", 0);
                        eb.pushScope(Scope.Class);
                        //eb.invalidTypeName();
                    }
                    break;
                case '}':
                    try {
                        if (eb._scope == Scope.Method) eb.pushScope(Scope.AbstractMethod);
                        //else if (eb._scope == Scope.Type) eb.pushScope(Scope.);
                        eb.popScope();
                    } catch (InvalidOperationException ioe) {
                        Console.WriteLine(ioe.Message);
                        ioe.Message.AppendLine(log);
                        eb.forcePop();
                    }
                    break;
            }
            pos++;
            return;
        }

        static ParserStorage ps;

        /// <summary>
        /// This walker is used for actual code parsing, and handles nested scopes as well. <para/>
        /// Requires: list[pos] == '{' AND correct Scope.## Returns: list[pos] == '}'
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int newScope(ref ExeBuilder eb, ref int pos)
        {
            if ((eb._scope & (Scope.MethodBody | Scope.Nested)) == 0) {
                throw new InvalidOperationException("Can only open scopes inside members of type \"method-group\"");
            }
            var list = eb.list;
            if (list.nextIs('}')) return ++pos;
            string elem;// = list[pos];
            Keyword.del func;
            // Maybe managing variables here as well? (Think it is a bad idea, but would make 'loc not declared' easier.)
            try {
                while (true) {
                    elem = list[pos];
                    if (elem.EqualsChar(';')){ pos++; continue; }
                    if (kw.TryGetValue(elem, out func)) pos = func.Invoke(ref eb, ref pos);
                    else if (eb.resolveType(elem, false) != null) pos = kw["void"].Invoke(ref eb, ref pos);
                    else if (elem.EqualsChar('}')) /**/ break;/*/ { pos++; eb.popScope(); break; } //*/
                    else pos = kw["_expr"].Invoke(ref eb, ref pos);
                    //throw new InvalidSyntaxException("PARSER", elem, pos);
                    /// See ParserStorage_Code: eval ExprObj (located in ExeBuilder) and write to IL Stream.
                }
            } catch (Exception e) // This is one hell of a catch block
                when (e is InvalidSyntaxException || e is InvalidOperationException) {
                if (doDebug) throw;
                if (e.Message.Contains("PARSER")) throw;
                if (e.Message.Contains("<Invalid>")) {
                    int debugHook = 0;
                }
                CatchHandler(ref eb, ref eb.list.Index, e);
            }/* catch (Exception e) { }//*/
            return pos;
        }

        /// <summary>
        /// Parse a isolated expression, mostly inline expressions for ControlFlow or '=>' (?) Returns on ';' <para/>
        /// Requires: Valid expression ## Returns: list[pos] == ';' ## This should be optimized for ++/--, and maybe for Conditionals <para/>
        /// See Remarks for more information.
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        /// <remarks>
        /// For short, this is just "newScope" but without the loop. When a use is found, this will most likely be refractored to match that use.
        /// As it is, I think it wouldn't work and is half-assed the original and ... something else.
        /// </remarks>
        public static int inlineExpr(ref ExeBuilder eb, ref int pos)
        {
            if ((eb._scope & (Scope.MethodBody | Scope.Nested)) == 0) {
                throw new InvalidOperationException("Can only open scopes inside members of type \"method-group\"");
            }
            var list = eb.list;
            if (list.nextIs(';')) return ++pos;
            string elem = list[pos];
            Keyword.del func;
            // Maybe managing variables here as well? (Think it is a bad idea, but would make 'loc not declared' easier.)
            try {
                if (kw.TryGetValue(elem, out func)) pos = func.Invoke(ref eb, ref pos);
                else if (Type.GetType(elem, false) != null) pos = kw["void"].Invoke(ref eb, ref pos);
                else pos = kw["_expr"].Invoke(ref eb, ref pos);
                    //throw new InvalidSyntaxException("PARSER", elem, pos);
            } catch (Exception e) // This is one hell of a catch block
                when (e is InvalidSyntaxException || e is InvalidOperationException) {
                if (doDebug) throw;
                if (e.Message.Contains("PARSER")) throw;
                if (e.Message.Contains("<Invalid>")) {
                    int debugHook = 0;
                }
                CatchHandler(ref eb, ref eb.list.Index, e);
            }/* catch (Exception e) { }//*/
            return pos;
        }
#endif
    }
}
