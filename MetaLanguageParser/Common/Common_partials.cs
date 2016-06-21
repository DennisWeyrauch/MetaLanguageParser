using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    class Reflection_partial
    {
        static bool doDebug = false;
        #region Escaping Code
        /// <summary>
        /// Remove LineBreaks, Tabs and LineComments (except if they are in Strings)
        /// BlockComments and surplus Space is ignored anyway by the Compiler
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static string escapeCode(string code, bool debug = false)
        {
            doDebug = debug;
            foundCond = false;
            foundDebug = false;
            if (System.IO.File.Exists("code_with_condComp.cs")) System.IO.File.Delete("code_with_condComp.cs");
            if (doDebug) System.IO.Directory.CreateDirectory("debug");
            string escStr = @"(""((?>\\\\)|(?>\\"")|[^""])*?"")";
            if (debug) code.WriteText(@"debug\logA_raw.cs");

            /// Step1: Remove comments (String | BlockComment | [Stacked] LineComments)
            // [^\\] for Escape
            var c1 = Regex.Replace(code, escStr + @"|(?:/[*][\s\S]*?[*]/)|(?://.*?(?:(?:\s*//.*?)*)?)(\r\n)", new MatchEvaluator(evalStr));//"$1");
            if (debug) c1.WriteText(@"debug\logB_noComments.cs");

            /// Step2: Remove Preprocessor Directives (Transform Conditional, remove rest)
            var c2 = Regex.Replace(c1, escStr + @"|#if([\s\S]*?)#endif", new MatchEvaluator(doEvalCond), RegexOptions.Multiline);
            c2 = Regex.Replace(c2, escStr + @"|^\s*#.*\r\n|\t+", "$1", RegexOptions.Multiline);
            if (debug) c2.WriteText(@"debug\logC_noPraePro.cs");
            if (foundCond) {
                Console.WriteLine("Found Conditional compilation directives. Compilation may fail");
                c2.WriteText(@"code_with_condComp.cs");
            }
            if (foundDebug) {
                Console.WriteLine("Found DEBUG compilation directives. Reduced Code according to debug argument '-d / debug'.");
                c2.WriteText(@"code_with_condComp.cs");
            }
            string result = c2;
            return result;
        }

#if false // escapeCode with Comments
        internal static string escapeCode(string code, bool debug = false)
        {
            doDebug = debug;
            foundCond = false;
            foundDebug = false;
            if (System.IO.File.Exists("code_with_condComp.cs")) System.IO.File.Delete("code_with_condComp.cs");
            if (doDebug) System.IO.Directory.CreateDirectory("debug");
            //string escStr = "(?(?=@\")(@\"[^\"]*?\"(?:\"[^\"]*?\")*)|(\\$?\"[^\"]*?((?>\\\")[^\"]*|\")))";//@"(""[\s\S]*?(?!\\"")"")";
            //string escStr = "((?>(?(?=@\")(?:@\"[^\"]*?\"(?:\"[^\"]*?\")*)|(?:[$]?\"[^\"]*?(?:(?>\\\")[^\"]*|\")))))";//@"(""[\s\S]*?(?!\\"")"")";
            string escStr = @"(""((?>\\\\)|(?>\\"")|[^""])*?"")";//"(\"([^\\](?>\\\")|[^\"])*?\")";
            if (debug) code.WriteText(@"debug\logA_raw.cs");

            /// Step1: Remove comments (String | BlockComment | [Stacked] LineComments)
            // [^\\] for Escape
            var c1 = Regex.Replace(code, escStr + @"|(?:/[*][\s\S]*?[*]/)|(?://.*?(?:(?:\s*//.*?)*)?)(\r\n)", new MatchEvaluator(evalStr));//"$1");
            if (debug) c1.WriteText(@"debug\logB_noComments.cs");

            /// Step2: Remove Preprocessor Directives (Transform Conditional, remove rest)
            // var c2 = Regex.Replace(c1, @"(""[\s\S]*?"")|^\s*#if [\s\S]*?\r\n\s*(?:#else([\s\S]*?)\r\n|#endif)|^\s*#.*\r\n|\t+", "$1$2", RegexOptions.Multiline); // Not feasable to findout which part should be compiled 
            var c2 = Regex.Replace(c1, escStr + @"|#if([\s\S]*?)#endif", new MatchEvaluator(doEvalCond), RegexOptions.Multiline);
            c2 = Regex.Replace(c2, escStr + @"|^\s*#.*\r\n|\t+", "$1", RegexOptions.Multiline);
            if (debug) c2.WriteText(@"debug\logC_noPraePro.cs");
            if (foundCond) {
                Console.WriteLine("Found Conditional compilation directives. Compilation may fail");
                c2.WriteText(@"code_with_condComp.cs");
            }
            if (foundDebug) {
                Console.WriteLine("Found DEBUG compilation directives. Reduced Code according to debug argument '-d / debug'.");
                c2.WriteText(@"code_with_condComp.cs");
            }
            string result = c2;
            
            /// Step3: Organize usings
            /*var matches = Regex.Matches(c2, @"^(using (?!static).*?;)", RegexOptions.Multiline).Cast<Match>();
            string usings = "";
            foreach (var token in from m in matches select m.Groups[1].Value) {
                usings += token + "\r\n";
            }
            var c3 = Regex.Replace(c2, escStr + @"|^using (?!static).*?;\s*?$", "$1", RegexOptions.Multiline);
            var c4 = usings + c3;
            if (debug) c4.WriteText(@"debug\logD_usings.cs");
            //*/

            /// Step5: Remove all Tabs and all Newlines
            /*if (doDebug) return Regex.Replace(c4, @"\t+|(@""[^""]*?""(?:""[^""]*?"")*)|\r\n", "$1");
            else return Regex.Replace(c4, @"\t+|(@""[^""]*?""(?:""[^""]*?"")*)|\r\n( )+", "$1$2");//*/
            return result;
        }
        // Verbatim: (@\"[^\"]*?\"(?:\"[^\"]*?\")*) // (@""[^""]*?""(?:""[^""]*?"")*)
        // String: (?!@")("(\\"|[^"]*)(?<!\\)")
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string escapeEmbedded(string code)
        {
            string escStr = @"(""((?>\\\\)|(?>\\"")|[^""])*?"")";//"(\"([^\\](?>\\\")|[^\"])*?\")"
            return Regex.Replace(code, escStr + @"|(?:/[*].*?[*]/)",  new MatchEvaluator(evalInter), 
                RegexOptions.Singleline | RegexOptions.Compiled);
            //return Regex.Replace(code, escStr + @"|(?:/[*][\s\S]*?[*]/)|(?://.*?(?:(?:\s*//.*?)*)?)(\r\n)", new MatchEvaluator(evalStr));
        }
        // 32 byte, 0 locals, maxstack 8
        static string evalInter(Match m) => (Regex.IsMatch(m.Value, "^[@$]?\"[\\s\\S]*\"$")) ? m.Value : "";
        // 41 byte, 2 locals, maxstack 2
        static string evalStr(Match m)
        {
            if (Regex.IsMatch(m.Value, "^[@$]?\"[\\s\\S]*\"$")) {
                return m.Value;
            }
            return "\r\n";
        }

        static bool foundCond = false;
        static bool foundDebug = false;

        public static string replaceCondPre(string code)
        {
            return Regex.Replace(code, @"(""((?>\\\\)|(?>\\"")|[^""])*?"")|" + @"#if([\s\S]*?)#endif", new MatchEvaluator(doEvalCond), RegexOptions.Multiline);
        }
        static string doEvalCond(Match match)
        {// ^\s*#if (.*?)\r\n([\s\S]*?)(?=#else|#elif|#endif)|#else ([\s\S]*?)(?=#endif)
            var org = match.Value;
            if (Regex.IsMatch(org, "^[@$]?\"[\\s\\S]*\"$")) {
                return org;
            }
            List<string[]> list = new List<string[]> {
                //new[] {@"(""[\s\S]*?"")", "$1"}, // strings
                new[] {@"#if\s*(.*?)\r\n([\s\S]*?)(?=#elif|#else|#endif)", "if ($1) {\r\n$2" }, // #if
                new[] {@"#elif\s*(.*?)\r\n([\s\S]*?)(?=#elif|#else|#endif)", "} else if ($1) {\r\n$2" }, // #elif
                new[] {@"#else([\s\S]*?)(?=#endif)", "} else {\r\n$1"}, // #else
                new[] {@"#endif", @"}"}  // #endif
            };

            #region At least make Debug work
            int idx = -1; // idx ==> Throw away all except list[idx]
            Dictionary<string, int> dict = new Dictionary<string, int>() {
                { @"#if\s*true", 0 }, { @"#if\s*false", 2 },
                { @"#if\s*DEBUG",   (doDebug)?0:2}, { @"#if\s*!DEBUG",   (doDebug)?2:0},
                { @"#elif\s*DEBUG", (doDebug)?1:2}, { @"#elif\s*!DEBUG", (doDebug)?2:1},
            };
            foreach (var item in dict) { if (Regex.IsMatch(org, item.Key)) { idx = item.Value; break; } }
            if (idx != -1) {
                foundDebug = true;
                var rep = (idx == 2)?"$1" : "$2"; // #else has only one ()-pair
                for (int i = 0; i < list.Count; i++) {
                    list[i][1] = (i == idx) ? rep : "";
                }
            } else foundCond = true;
            #endregion
            for (int i = 0; i < list.Count; i++) {
                org = Regex.Replace(org, list[i][0], list[i][1]);
            }
            return org;
        }

        #endregion
    }
    /*
    static class Extensions_partial
    {
        /// <summary>
        /// Creates or overwrites File at '<paramref name="path"/>' to write into.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        public static void WriteText(this string str, string path) => System.IO.File.WriteAllText(path, str);
    }//*/
}
