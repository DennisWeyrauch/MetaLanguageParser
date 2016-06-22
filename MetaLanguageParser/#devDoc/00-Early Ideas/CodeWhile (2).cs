using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaLanguageParser;
using MetaLanguageParser.Operands;
using Common;

namespace MetaLanguageParser.MetaCode
{
    class CodeWhile : CodeBase, ICode
    {
        public string FileName => "while";
        public string Name => "While";

        string code => @"
while (i > 0) {
    i--;
    $$write(i)$$
}";
        string metacode => @"
while (> i 0) {
    (dec i);
    $$write(i)$$
}";


        string raw => @"
while ($$cond$$) {$§inc$
    $$code$$ $§dec$
}";

        List<string> readin = new List<string>() {
            "while (", "cond", ") {", "§inc", "§n", "code", "§dec", "§n", "}"
        };
        //public static Dictionary<string, string> dict;

        public string parse(ref ExeBuilder eb, ref int pos)
        {
            var list = eb.list;

            readin = this.readFile(ref eb, FileName); // eb since it contains the current LangInstance
            dict.Add("i", "i");

            pos++; // Skip keyword.
            string elem = list[pos];
            list.assertC("("); // .assertC("$$cond$$")
            
            dict.Add("cond", readConditional(ref eb, ref pos));

            list.assertC(")").assert("{");
            
            dict.Add("code", readAnyCode(ref eb, ref pos));

            list.assert("}");

            //list.getNext();
            return buildCode(readin, dict, ref eb.Indent);
        }
    }
}
