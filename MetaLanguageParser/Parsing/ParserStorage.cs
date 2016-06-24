using MetaLanguageParser.MetaCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser
{
    class ParserStorage
    {
        internal Dictionary<string, FuncDel> fillLists()
        {
            var dict = new Dictionary<string, FuncDel>();
            dict.Add("while", new CodeWhile().parse);

            return dict;
        }
    }
}
