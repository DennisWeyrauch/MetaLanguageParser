using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parsing.Adapter;

namespace MetaLanguageParser.MetaCode
{
    class Assign
    {
        /// <summary>
        /// §assign(i)(value);
        /// </summary>
        internal static string parse(ref int pos)
        {
            var list = getList();
            list.assertC("§assign").assertC("(");
            string name = list[pos++];
            list.assertC(")").assertC("(");
            string value = readExpression();
            list.assertC(")").assertC(";");

            return string.Format(LocalData.forStr_assign, name, value);
        }
    }
}
