using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parsing.Adapter;

namespace MetaLanguageParser.MetaCode
{
    class AddType
    {
        /// <summary>
        /// §addType(§main)  ---    /// Adds a EntryType like in "/myLang/§mainType.{suffix}"
        /// </summary>
        internal static string parse(ref int pos)
        {
            TypeData data;
            var list = getList();
            pos++;
            list.assertC('('); // Skip §addMethod
            if (list[pos].Equals("§main")) {
                data = TypeData.setMain();
                pos++;
                list.assertC(')');
            } else throw new NotImplementedException("addType.AnythingElse");
            currentType = data;

            var ret = execRun("§endType");
            data.setStuff(ret);
            pos++; // exec should return on §endType, so skip that
            
            AddType(data);
            currentType = null;

            return "";
        }
    }
}
