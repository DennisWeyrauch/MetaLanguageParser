using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parsing.Adapter;

namespace MetaLanguageParser.MetaCode
{
    class AddMethod
    {
        // §addMethod()(myMethod) /// Adds a method called "myMethod" with no modifiers or Parameters, + ReturnType = void

        /// <summary>
        /// §addMethod(§main) --- /// Adds a EntryMethod like in "/myLang/§main.{suffix}"
        /// </summary>
        internal static string parse(ref int pos)
        {
            MethodData data = new MethodData(); // Should be embedded Type
            var list = getList();
            pos++;
            list.assertC("(");
            if (list[pos].Equals("§main")) {
                data.setMain();
                pos++;
                list.assertC(")");
            }
            else data.readSignature(ref list, ref pos);
            // Put the indetn outside
#warning Also handle somehow the §main contents (Maybe own Handler for that (and §program as well))
#warning Called from destFiles, this should just take the lines as they are without parsing (until §endMethod)
            if (!data.isSigOnly()) {
                currentMethod = data;
                var ret = execRun("§endMethod");
                data.setCode(ret);
                pos++; // exec should return on §endMethod, so skip that

                data = currentMethod; // ??
                currentMethod = null;
            }
            AddMethod(data);

            return "";
        }
    }
}
