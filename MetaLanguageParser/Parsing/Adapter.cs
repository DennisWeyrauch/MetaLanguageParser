using Common;
using MetaLanguageParser.MetaCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.Parsing
{
    public static class Adapter
    {
        // Look into errorLog Copy
        // Over all work with everything internal, and then set either stuff on public (Data) or via Adapter (Exebuilder)
        /*
        CodeBase
            readExpression
        LocalData.class
        MethodData
            addLocal, setMain, setCode
        ExeBuilder
            AddLocal, AddMethod, AddType
        public static void ( data) => ExeBuilder.AddLocal(data);
        //*/

            /// <summary>
            /// Retrieve the ListWalker containing the code-to-parse
            /// </summary>
            /// <returns></returns>
        public static ListWalker getList() => ExeBuilder.Instance.list;

        public static void AddLocal(LocalData local) => ExeBuilder.AddLocal(local);
        public static void AddLocal(List<LocalData> locals) => ExeBuilder.AddLocal(locals);
        public static void AddMethod(MethodData data) => ExeBuilder.Instance.AddMethod(data);
        public static void AddType(TypeData data) => ExeBuilder.Instance.AddType(data);

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

    }
}
