﻿using System; // Console, DateTime
using System.Collections.Generic; // List<T>
using System.Globalization; // DateTimeInfo
using System.IO;   // StreamWriter, IOException, File, FileStream, enum FileMode, Directory
using System.Text; // StringBuilder
using System.Text.RegularExpressions; // MatchEvaluator, Match, enum RegexOptions, Regex
using Common;
using static MetaLanguageParser.Resources.ResourceReader;
//using static MetaLanguageParser.Resources.LangConfig;

namespace MetaLanguageParser//.Parsing
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

        static Parser()
        {
            kwDict = new Dictionary<string, FuncDel>();
            kwDict.Add("§addMethod", AddMethod.parse);
        }

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
            ListWalker list = toker.execute(file); // Slot 3/4
            ExeBuilder eb = null;

            StringWriter output = new StringWriter();

            System.CodeDom.Compiler.IndentedTextWriter writer = null;
            try {

                eb = new ExeBuilder(list, language); // Slot 4/4
                Resources.ResourceReader.readConfiguration(language);
                
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
                /// Rectract function
				string resCode = output.ToString();
				if(Regex.IsMatch(resCode, @"§retract\(\d+\)")){
					int hitPos = 0;
					foreach(var item in Extensions.matchHelper(resCode, @"§retract\((\d+)\)")){
						hitPos = resCode.IndexOf("§retract", hitPos);
						hitPos -= int.Parse(item);
                        int nextEnd = resCode.IndexOf(')', hitPos);
                        resCode = resCode.Remove(hitPos, nextEnd - hitPos+1);
                    }
				}
                try { File.WriteAllText($"Results.{__FILESUFFIX}", resCode); } catch (Exception) { Logger.logData("Could not write to OutputFile!"); }
				if (hasThrown) list.printError(finalize: true);
                writer?.Dispose();
                output?.Dispose();
            }
            _running = false;
        }






        static bool hasThrown = false;
        static int depthCnt = 0;
        public static string execRun(ref ExeBuilder eb, ref int pos, string terminator = "")
        {
            if (!_running) throw new InvalidOperationException("Calling recursive Parser Method without init!");
            var list = eb.list;
            string elem = "";
            depthCnt++;


            ICode ce = new CodeExample();
            var output = new StringWriter();
            var writer = new System.CodeDom.Compiler.IndentedTextWriter(output, "\t");
            FuncDel myfunc;
            while (true) {
                try {
                    elem = list.getCurrent();
                    if (kw.Contains(elem)) writer.Write(ce.parse(ref eb, ref pos));
                    else if (kwDict.TryGetValue(elem, out myfunc)) writer.Write(myfunc(ref eb, ref pos));
                    else if (list.isAtEnd(terminator)) {
                        //if (list.isClosure()) list.Index++;
                        elem = output.ToString();
                        break;
                    }
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
                    else if (list.isAtEnd()) {}
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
