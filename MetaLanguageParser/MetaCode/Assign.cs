using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.MetaCode
{
    class Assign
    {
        /// <summary>
        /// §assign(i)(value);
        /// </summary>
        internal static string parse(ref ExeBuilder eb, ref int pos)
        {
            var list = eb.list;
            list.assertC("§assign").assertC("(");
            string name = list[pos++];
            list.assertC(")").assertC("(");
            string value = CodeBase.readExpression(ref eb, ref pos);
            list.assertC(")").assertC(";");

            return string.Format(LocalData.forStr_assign, name, value);
        }
    }
}
