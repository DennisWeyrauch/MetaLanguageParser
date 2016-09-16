using Common;
using MetaLanguageParser.MetaCode;
using MetaLanguageParser.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.Parsing
{
    public static class Adapter
    {
        /// <summary>
        /// Retrieve the ListWalker containing the code-to-parse
        /// </summary>
        /// <returns></returns>
        public static ListWalker getList() => ExeBuilder.Instance.list;

        /// <summary>
        /// Read any File in form of "Key   Value" (Tab inbetween), and return lines as KeyValuePairs of a Dictionary
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="lowerCase"></param>
        /// <returns></returns>
		public static Dictionary<string, string> readFile(string path, bool lowerCase = false) => ResourceReader.readAnyFile(path, lowerCase);

        /// <summary>
        /// Read a codefile from "/lang" and return a tokenized list of the contents.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static List<string> readCodeFile(string filename) => new CodeBase().readFile(filename);

        /// <summary>
        /// Yielder for reading Files. Will skip any Empty/WhiteSpaceOnly Lines, or ones starting with '#'. Everything else will be split up at '\t' (Tab) and returned as string[]
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<string[]> readFilePerLine(string path) { return ResourceReader.readFile(path);}

        /// <summary> Add the given local to the current Method.</summary>
        /// <param name="local">The local to add</param>
        public static void AddLocal(LocalData local) => ExeBuilder.AddLocal(local);
        /// <summary> Add the given list of locals to the current Method.</summary>
        /// <param name="locals">The List to add</param>
        public static void AddLocal(List<LocalData> locals) => ExeBuilder.AddLocal(locals);
        /// <summary> Add the given Method to the current Type.</summary>
        /// <param name="data">The Method to add</param>
        public static void AddMethod(MethodData data) => ExeBuilder.Instance.AddMethod(data);
        /// <summary> Add the given Type to the list of generated Types.</summary>
        /// <param name="data">The Type to add</param>
        public static void AddType(TypeData data) => ExeBuilder.Instance.AddType(data);
		// Should handle all that is to Generics: Open/Close, Nesting, Filter. Syntax: <? extend/impl myClass, V extend Object>
        //public static void AddGenerics(IGeneric data) {}

        public static TypeData currentType { get { return ExeBuilder.Instance.currentType; } set { ExeBuilder.Instance.currentType = value; } }
        public static MethodData currentMethod { get { return ExeBuilder.Instance.currentMethod; } set { ExeBuilder.Instance.currentMethod = value; } }

        /// <summary>
        /// Recursive parserMethod to read MultiLineCodeblocks of Statements
        /// </summary>
        /// <param name="terminator"></param>
        /// <returns></returns>
        public static string execRun(string terminator = "")
        {
            var eb = ExeBuilder.Instance;
            return Parser.execRun(ref eb, ref getList().Index, terminator);
        }

        /// <summary>
        /// Read a syntactic code unit. For now, only Values/Operations supported
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string readExpression()
        {
            var eb = ExeBuilder.Instance;
            return CodeBase.readExpression(ref eb, ref getList().Index);
        }
		// TempSolution
	
		public static class SYMBOL {
			public static string Block = ResourceReader.__BLOCK_INIT;//"{";
            public static string Block_Close = ResourceReader.__BLOCK_CLOSE;//"}";
            public static string ListSeperator = ",";
            public static string Indent = ResourceReader.__INDENT;//"\t";
		}
		
		public static class KEYWORD {
			public static string Extend = "ext";
			public static string Implement = "impl";
		}
		
    }
}
