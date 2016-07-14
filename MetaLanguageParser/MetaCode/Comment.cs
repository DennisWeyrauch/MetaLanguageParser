using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.MetaCode
{
    class Comment
    {
        /// <summary>
        /// §assign(i)(value);
        /// </summary>
        internal static string parse(ref ExeBuilder eb, ref int pos)
        {
            var list = eb.list;
            list.assertC("§comment").assertC("(");
            string elem = list[pos++];
            string name = elem.Substring(1, elem.Length-2);
            list.assertC(")");

            return string.Format(Resources.ResourceReader.__COMMENT, name);
        }
    }
}
