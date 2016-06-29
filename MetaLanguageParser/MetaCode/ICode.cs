using MetaLanguageParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaLanguageParser.Operands;
using Common;
using System.Reflection;
using static MetaLanguageParser.Resources.ResxFiles;

namespace MetaLanguageParser.MetaCode
{
    public interface ICode
    {
        /// <include file='ICode.doc' path='ICode/Member[@name="parse"]/*' />
        //string Name { get; }
        /// <include file='ICode.doc' path='ICode/Member[@name="FileName"]/*' />
        //string FileName { get; }


        // <include file='ICode.doc' path='ICode/Member[@name="parse"]/*' />
        /// <summary>
        /// ENTRY: pos on KEYWORD #
		/// EXIT: pos on Last Token concerning me + 1
        /// </summary>
        /// <param name="eb">The Exebuilder reference</param>
        /// <param name="pos">The position inside the ListWalker</param>
        /// <example>
        /// var list = eb.list; pos++; // Skip keyword. string elem = list[pos];
        /// </example>
        string parse(ref ExeBuilder eb, ref int pos);
    }

    [System.Diagnostics.DebuggerDisplay("{FileName} (Cnt = {dict.Count})")]
    public class CodeExample : CodeBase, ICode
    {
        public string FileName { get; set; } = "meta";

        public string Name => "Meta";
        string code => @"
while ($$cond$$) {
    $$code$$
}";

        /// <summary>
        /// Storage Dictionary to save previously tokenized KeyWordFiles for reuse.
        /// </summary>
        static Dictionary<string, List<string>> baseDict = new Dictionary<string, List<string>>();
        public static List<string> getEntry(string elem)
        {
            if (baseDict.ContainsKey(elem)) return baseDict[elem];
            else {
                var tokens = Tokenizer.TokenizeFile(getMetaPath(elem), Tokenizer.enumType.MetaDef, false);
                baseDict.Add(elem, tokens);
                return tokens;
            }
        }

        public string parse(ref ExeBuilder eb, ref int pos)
        {
            var list = eb.list;
            string elem = list[pos]; // keyword. If unsynced, use AdapterDict
            FileName = elem;
            // Contains the metaDefinition code
            ListWalker tokens;
            if (baseDict.ContainsKey(elem)) tokens = new ListWalker(baseDict[elem]);
            else {
                tokens = Tokenizer.TokenizeFile(getMetaPath(elem), Tokenizer.enumType.MetaDef, false);
                baseDict.Add(elem, tokens);
            }
            /*
            // Debug Hook
            if (elem.Equals("§write")) {
                new Object();
            }//*/

            // Contains the metaDestination code
            var readin = readFile(FileName); // eb since it contains the current LangInstance
            
            /*if(eb.codeBase.Count == 2) {
                var obj = new Object();
            }//*/
            while (!tokens.isAtEnd()) {
                  //  new Object();
                if (tokens.isCurrent("$$")) {
                    string key = tokens.getNext();   /// $$ -> Key   -> [ASSIGN]
                    tokens.assertPreInc("::");       /// Key -> ::   -> [TEST]
                    string value = tokens.getNext(); /// :: -> Value -> [ASSIGN]
                    tokens.assertPreInc("$$");       /// Value -> $$ -> [TEST]
                    tokens.Index++;                  /// $$ -> ...
                    eb.codeBase.Push(this);
                    string result = metaDict[value].Invoke(ref eb, ref pos);
                    eb.codeBase.Pop();

                    dict.Add(key, result);
                }

                list.assertNoInc(tokens.getCurrent()); 
                pos++;
                tokens.Index++;
            }

            return buildCode(readin, dict);
        }
    }
}
