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
    using MetaCode.TypeWriter;
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
        //static string basePath = Resources.ResxFiles.basePath;
        //string input = "";
        Tokenize.Tokenizer toker;
        public static bool scopeChanged = false;
        public static Dictionary<string,CodeDel> kwDict;
        public static List<string> kw;
        public static int depth = 0;
        public static Parser _instance;
        public static Parser getInstance => _instance;
        internal static System.Reflection.Assembly codeAsm;

        static Parser()
        {
            Console.WriteLine("Checking MetaCode directory...");
            string path = "MetaCode";
            string lib = "MetaLib.dll";
            string libPath = path+"/"+lib;
            string linkPath = path+"/Linker.txt";
            Common.Reflection.Reflection.setSuppressor(Program.suppressError);

            bool exists = File.Exists(libPath);
            FileInfo libFile = null;
            try {
                libFile = (exists) ? new FileInfo(libPath) : null;
            } catch (FileNotFoundException) { exists = false; }

            var fileDict = readAnyFile(linkPath);
            var dirFiles = Directory.EnumerateFiles(path);
            var fileList = new List<string>();
            foreach (var item in dirFiles) {
                FileInfo fi = new FileInfo(item);
                Tokenize.Tokenizer.fixEncodingErrors(item);
                string file = fi.Name.Substring(0,fi.Name.Length-fi.Extension.Length);
                if (fileDict.ContainsKey(file)) {
                    fileList.Add(File.ReadAllText(item));
                    if (exists && libFile.LastWriteTime < fi.LastWriteTime) exists = false;
                }
            }
            if (exists && libFile.LastWriteTime < new FileInfo(linkPath).LastWriteTime) exists = false;
            if (exists) {
                Console.WriteLine("Loading last recent Library...");
                codeAsm = System.Reflection.Assembly.LoadFrom(libFile.FullName);
            } else {
                Console.WriteLine("Compiling new MetaLib...");
                //fileList.Add($"public class X {{ public static int cnt = {fileList.Count}; }}");
                codeAsm = Common.Reflection.Reflection.getAssembly(fileList, true, lib);
                if (codeAsm != null) {
                    File.Delete(libPath + "_old");
                    File.Replace(lib, libPath, libPath + "_old");
                } else {
                    Console.WriteLine("Could not create Code-Assembly!");
                    if (File.Exists(libPath)) {
                        Console.WriteLine("Loading Backup Lib !");
                        codeAsm = System.Reflection.Assembly.LoadFrom(libFile.FullName);
                    }
                }
            }

            Type tempType;
            kwDict = new Dictionary<string, CodeDel>();

            fileDict.Remove("AddType"); kwDict.Add("§addType", AddType.parse);

            foreach (var item in fileDict) {
                tempType = codeAsm.GetType("MetaLanguageParser.MetaCode."+item.Key);
                var del = tempType.GetMethod("parse", Common.Reflection.Reflection.LookupAll).CreateDelegate(typeof(CodeDel));
                kwDict.Add(item.Value, (CodeDel) del);
                //kwDict.Add(item.Value, (CodeDel)(codeAsm.GetType("MetaLanguageParser.MetaCode." + item.Key)).GetMethod("parse", Common.Reflection.Reflection.LookupAll).CreateDelegate(typeof(CodeDel)));
            }
            Console.WriteLine($"Done. ({fileDict.Count} loaded)");
        }

        public Parser(bool debug = false)
        {
            //doDebug = Program.doDebug; // static set via main
            //StringBuilder sb = new StringBuilder().Append(File.ReadAllText(files[0]));
            doDebug = debug;
            toker = new Tokenize.MetaTokenizer();
            _instance = this;
            kw = new List<string>();
        }


        /// <summary>Main trigger method to start building</summary>
        public void execute(string file, string language, bool readConfigs = true) // 111
        {
            if (_running) {
                Console.WriteLine("Another instance already exists!");
                return;
            } else {
                Console.WriteLine($"[Settings] Lang: {language}, CodeFile: {file}");
                Console.WriteLine("Tokenizing...");
                _running = true;
            }
            int len; // Reserving Slots for locals.
            ListWalker list = toker.execute(file); // Slot 3/4
            ExeBuilder eb = null;

            StringWriter output = new StringWriter();

            System.CodeDom.Compiler.IndentedTextWriter writer = null;
            try {
                eb = ExeBuilder.getInstance(list, language); // Slot 4/4
                Console.WriteLine(); 
                if (Program.printParts) Program.deleteParts();
                
                writer = new System.CodeDom.Compiler.IndentedTextWriter(output, __INDENT);

                kw.AddRange(Directory.EnumerateFiles(Resources.ResxFiles.getMetaPath()).Select(Path.GetFileNameWithoutExtension));
                //kwDict = new ParserStorage().fillLists();
                kw.Remove(language);
                len = list.Count;
#warning
                //eb.currentMethod
                Console.WriteLine("Starting Execution...");
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
                /// Rectract function
                //string resCode = // TypeArr.toString()...

                var sb = new StringBuilder( );
                // Add Imports // 
                // Add C-Predeclare Signatures //
#warning If CStyle with one pass, first go through methDict and add all signatures
#warning Then go through the Types (which is missing currently)
                Console.WriteLine("Building Types...");
				//*
				TypeWriter tw = TypeWriter.Factory(language);
                string typeStrings = "__COULD_NOT_OPEN_TYPEWRITER__";
                try {
                    typeStrings = tw.writeTypes(eb.typeDict);
                } catch (Exception e) {
                    Console.WriteLine("ERROR: There were errors during printing the code. (Details in ErrorLog)");
                    Logger.logException(e);
                    /*/
                    foreach (var item in eb.typeDict) {
                        sb.AppendLine(item.Value.ToString());
                    }
				    //*/

                }

                Console.WriteLine("Cleaning up...");
                
				if(Regex.IsMatch(typeStrings, @"§retract\(\d+\)|§EOF", RegexOptions.Multiline)){
					int hitPos = 0;
					foreach(var item in Extensions.matchHelper(typeStrings, @"§retract\((\d+)\)")){
						hitPos = typeStrings.IndexOf("§retract", hitPos);
						hitPos -= int.Parse(item);
                        int nextEnd = typeStrings.IndexOf(')', hitPos);
                        typeStrings = typeStrings.Remove(hitPos, nextEnd - hitPos+1);
                    }
                    typeStrings = Regex.Replace(typeStrings, "([ \t]*§EOF)", "", RegexOptions.Multiline);
				}

                string resCode = output.ToString() + typeStrings;
                try { File.WriteAllText($"Results.{__FILESUFFIX}", resCode); File.Delete($"WriteError.{__FILESUFFIX}"); } catch (Exception e) {
                    Logger.logData(new StringBuilder("Could not write to OutputFile!").AppendLine().Append(e.Message).ToString());
                    File.WriteAllText($"WriteError.{__FILESUFFIX}", resCode);
                }
				if (hasThrown) list.printError(finalize: true);
                writer?.Dispose();
                output?.Dispose();
            }
            _running = false;
        }


        static bool hasThrown = false;
        static int depthCnt = 0;
        /// <summary>
        /// Recursive parserMethod to read MultiLineCodeblocks of Statements
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <param name="terminator"></param>
        /// <returns></returns>
        public static string execRun(ref ExeBuilder eb, ref int pos, string terminator = "")
        {
            if (!_running) throw new InvalidOperationException("Calling recursive Parser Method without init!");
            var list = eb.list;
            string elem = "";
            depthCnt++;


            ICode ce = new CodeWalker();
            var output = new StringWriter();
            var writer = new System.CodeDom.Compiler.IndentedTextWriter(output, "\t");
            //FuncDel myfunc;
            CodeDel myfunc;
            while (true) {
                try {
                    elem = list.getCurrent();
                    // Valid Statements: §vardecl, §assign, §pre/postinc/dec, §call, and keywords
                    if (kw.Contains(elem)) writer.Write(ce.parse(ref eb, ref pos));
                    else if (kwDict.TryGetValue(elem, out myfunc)) {
                        if (elem.Equals("§comment")) {
                            elem = myfunc(ref pos);//writer.Write(myfunc(ref eb, ref pos));
                            if (elem.IsNotNOE()) writer.InnerWriter.WriteLine(elem);
                        } else {
                            elem = myfunc(ref pos);//writer.Write(myfunc(ref eb, ref pos));
                            if (elem.IsNotNOE()) writer.WriteLine(elem);
                        }
                    } else if (list.isAtEnd(terminator)) {
                        //if (list.isClosure()) list.Index++;
                        elem = output.ToString();
                        break;
                    } else throw new InvalidSyntaxException("PARSER", elem, pos);
                } catch (Exception e)
                    when (e is InvalidSyntaxException || e is InvalidOperationException) {
                    if (doDebug) throw;
                    if (e.Message.Contains("PARSER")) {
                        //if (!hasThrown)
                            Console.WriteLine(e.Message);
                        throw;
                    }
                    list.printError();
                    hasThrown = true;
                    writer.Write("<<<ERROR>>>");
                    list.skipBalanced('{', '}');

                    //if (e.Message.Contains("<Invalid>")) { int debugHook = 0; }
                    //CatchHandler(ref eb, ref eb.list.Index, e);
                }/* catch (Exception e) { }//*/
            }
            //writer.WriteLine(@"/** fgfgfg**/"); elem = writer.ToString();
            depthCnt--;
            //if(hasThrown && depthCnt == 0) list.printError(finalize: true);
            writer.Dispose();
            output.Dispose();
            return elem;
        }

        /// A Statement is one of the following things
        /// - An Variable Declaration: "int tz" or "int a, b, c"
        /// -- Optional chained with the Assignment of Expression(s)
        /// - Assignment of any local, field, or property in scope, with varying levels of dereference
        /// - Method Call
        /// - new()-Calls
        /// - Increment/Decrement
        /// - Throw, Return, Break, Continue
        /// - ControlFlow Structures like if, for, try/catch, etc.
        public static string execStatement(ref ExeBuilder eb, ref int pos) // , bool isStatement = false
        {
            if (!_running) throw new InvalidOperationException("Calling Statement Parser Method without init!");
            var list = eb.list;
            string elem = "";
            CodeDel func;
            ICode ce = new CodeWalker();
			var str = "";
            //while (true) {
                try {
                    elem = list.getCurrent();
					#warning INFO:: Added Dictionary as else; add §varDecl, §lambda, etc. with different methods
                    if (kw.Contains(elem)) str = ce.parse(ref eb, ref pos);
                    else if (list.isAtEnd()) {}
					#warning INFO:: Add something for Assign/MethodCall to use X=y and X.Y instead of §assign and §call
					//else if(varDict...) str = ... // Assignement/MethodCall
					else if(kwDict.TryGetValue(elem, out func)) str = func.Invoke(ref pos);
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
                str += __STATEMENT_CLOSE;
                pos++;
            }
            //if (hasThrown && depthCnt == 0) list.printError(finalize: true);
            /*elem = output.ToString();
            writer.Dispose();
            output.Dispose();//*/
            return str;
        }

        /// Expression is one of the following things
        /// - A Value, which can be
        /// -- A literal (0, 2.0, -4, 'c', "text", true)
        /// -- A Conditional expression resolving to a boolean value
        /// -- An Arithmetic expression
        /// -- A Ternary
        /// -- A lambda expression
        /// - A Reference
        /// -- this, null, new(), or new[] {}
        /// -- Method calls
        /// -- Any Local/Field/Property in scope (or via Proxy)
        /// - An Assignment with Closure and again an Expression of its own: (i = ....)
        public static string execExpression(ref ExeBuilder eb, ref int pos) // , bool isStatement = false
        {
            if (!_running) throw new InvalidOperationException("Calling Expression Parser Method without init!");
            var list = eb.list;
            string elem = "";
            CodeDel func;
            ICode ce = new CodeWalker();
            var str = "";
            //while (true) {
            try {
                elem = list.getCurrent();
                if (kw.Contains(elem)) str = ce.parse(ref eb, ref pos);
                else if (list.isAtEnd()) { }
#warning INFO:: Add something for Assign/MethodCall to use X=y and X.Y instead of §assign and §call
                //else if(varDict...) str = ... // Assignement/MethodCall
                else if (kwDict.TryGetValue(elem, out func)) str = func.Invoke(ref pos);
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
                str += __STATEMENT_CLOSE;
                pos++;
            }
            //if (hasThrown && depthCnt == 0) list.printError(finalize: true);
            /*elem = output.ToString();
            writer.Dispose();
            output.Dispose();//*/
            return str;
        }

    }
}
