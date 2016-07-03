using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.MetaCode
{
    class AddType
    {
        /// <summary>
        /// §addType(§main)  ---    /// Adds a EntryType like in "/myLang/§mainType.{suffix}"
        /// </summary>
        internal static string parse(ref ExeBuilder eb, ref int pos)
        {
            TypeData data;
            var list = eb.list;
            pos++; // Skip §addMethod
            list.assertC("(");
            if (list[pos].Equals("§main")) {
                data = TypeData.setMain();
                pos++;
                list.assertC(")");
            } else throw new NotImplementedException("addType.AnythingElse");
            eb.currentType = data;

            var ret = Parser.execRun(ref eb, ref pos, "§endType");
            data.setStuff(ret);
            pos++; // exec should return on §endType, so skip that
            
            eb.AddType(data);
            eb.currentType = null;

            return "";
        }
    }
}
