using Common;
using MetaLanguageParser.Operands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MetaLanguageParser.Resources.ResourceReader;
using static Common.Extensions;
using static MetaLanguageParser.Resources.ResxFiles;

namespace MetaLanguageParser.MetaCode
{
    /// <summary>
    /// An empty base class to generate Methods into.
    /// Just to see what I'd need for things.
	/// Also, make partial AND own file: ReaderMethods(2nd) and Rest(Invoke, ReadFile, buildCode, etc.)
    /// </summary>
    public partial class CodeBase
    {

        /// <summary>
        /// Contains:
        ///     basePath, FuncDict
        ///     readFile, buildCode : mdef->List / Token+Dict->Code
        ///     parseNumeric : Value
        ///     resolveIfExisting
        /// </summary>

        #region Initialisation Methods

        /// <summary>
        /// Change to TriState with additional Local, Arg, Field, Method. <para/>
        /// Currently only for $$code$4 and such. And emptied after BuildCode
        /// </summary>
        protected Dictionary<string, string> dict = new Dictionary<string, string>();

        //protected Dictionary<string, string> memberDict = new Dictionary<string, string>();


        protected static Dictionary<string, FuncDel> metaDict;
        static CodeBase()
        {
            metaDict = new Dictionary<string, FuncDel>() {
                {"readConditional", readConditional },
                {"readCode", readStatements },
                {"readAnyCode", readAnyCode },
                {"readParameterList", readParameterList },
            };
            
            System.IO.Directory.GetFiles(".", "myLog_*").ToList().ForEach(s => System.IO.File.Delete(s));
        }

        /// <summary>
        /// Tokenize with METATYPE # Copy items: Remove $$, \r\n into §n, rest no changes
        /// </summary>
        /// <param name="eb"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal List<string> readFile(ref ExeBuilder eb, string fileName)
        {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            List<string> readin = new List<string>();
            string readin1 = "while ($$cond$$) { §inc\r\n\t$$code$$ §dec\r\n}";

            var tokens = Tokenizer.TokenizeFile(getLangPath(fileName), Tokenizer.enumType.MetaType, false);

            List<string> readin2 = new List<string>() {
                 "while (", "$$", "cond", "$$", ") {", "§inc", "\r\n", "$$", "code", "$$", "§dec", "\r\n", "}"
            };
            foreach (var item in tokens) {
                if (item.Equals("$$")) continue;
                if (item.Equals("\t")) continue;
                if (item.Equals(" ")) continue;
                if (item.Equals("    ")) continue;
                //readin.Add((item.Contains('\r') || item.Contains('\n')) ? "§n" : item);
                if (item.Contains('\r') || item.Contains('\n')) readin.Add("§n");
                else readin.Add(item);
            }

            List<string> readin3 = new List<string>() {
                 "while (", "cond", ") {", "§inc", "§n", "code", "§dec", "§n", "}"
            };

            return readin;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
        }
#endregion


        /// <see cref="Common.Extensions.IsNumeric(object)"/>
        static internal Value parseNumeric(string elem)
        {
            int i;
            long l;
            //decimal dec;
            float f;
            double d;
            bool b;
            if (Int32.TryParse(elem, out i)) return new Value(i);
            if (Int64.TryParse(elem, out l)) return new Value(l);
            //if (Decimal.TryParse(elem, out dec)) return new Value(dec);
            if (Double.TryParse(elem, out d)) return new Value(d);
            if (Single.TryParse(elem, out f)) return new Value(f);
            if (Boolean.TryParse(elem, out b)) return new Value(b);
            throw new NotImplementedException("Invalid Numeric Type in parseNumeric");
        }

        // If a name (local, argument, method/field/etc.) exists, return the full reference (method with () )
        // Stuff like "This", WRITE, READ is among that as well.
        // And don't care about "out of scope" or thelike. The progLang it self should handle that
        internal bool resolveIfExist(string elem, out string str)
        {
            return dict.TryGetValue(elem, out str);
        }

        /// <summary>
        /// Final method to build the destination code after reaching the end of the codeFragment 
        /// </summary>
        /// <param name="readin"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        internal string buildCode(List<string> readin, Dictionary<string, string> dict, ref int ebInt)
        {
            /*List<string> readin2 = new List<string>() {
                 "while (", "cond", ") {", "§inc", "§n", "code", "§dec", "§n", "}"
            };//*/

            if (((CodeExample)this).FileName.Equals("else")) {
                new Object(); // DebugHook
            }
            bool isRetract = false;

            using (var output = new System.IO.StringWriter())
            using (var writer = new System.CodeDom.Compiler.IndentedTextWriter(output, __INDENT)) {
                writer.Indent = ebInt;
                foreach (var item in readin) {
                    //System.IO.File.WriteAllText($"myLog_{((CodeExample)this).FileName}.txt", output.ToString()); // Prints Steps
                    if(isRetract) {
                        if (item.Contains(')')) isRetract = false;
                        writer.Write(item);
                        continue;
                    }
                    switch (item) {
                        case "§inc": writer.Indent++; continue;
                        case "§dec": writer.Indent--; continue;
                        case "§n": writer.WriteLine(); continue;
                        case "§retract":
                            writer.Write(item);
                            isRetract = true;
                            continue;
#warning INFO:: Add new custom functions like §addMember (like Method for ToCharArray in JS)
                    }
                    string s = "";
                    if (dict.TryGetValue(item, out s)) {
                        string indent;
                        switch (writer.Indent) {
                            case 0: indent = ""; break;
                            case 1: indent = __INDENT; break;
                            default:
                                indent = __INDENT.ConcatTimes(writer.Indent);
                                break;
                        }
                        // Apply the indent recursive to every code line
                        var str = Regex.Replace(s, "([\r\n]+(?:" + indent + ")*)", "${1}"+indent);
                        if (str.IsNotNOE()) writer.Write(str);
                    } else writer.Write(item);
                }

                ebInt = writer.Indent;
                dict.Clear();
                //System.IO.File.WriteAllText($"myLog_{DateTime.Now.ToFileTime()}_{((CodeExample)this).FileName}.txt", output.ToString()); // Prints Iterations
                return output.ToString();
            }
        }
    }
}
