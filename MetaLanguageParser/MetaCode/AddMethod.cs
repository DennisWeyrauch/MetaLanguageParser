using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.MetaCode
{
    class AddMethod
    {
        // §addMethod(§main)      /// Adds a EntryMethod
        // §addMethod()(myMethod) /// Adds a method called "myMethod" with no modifiers or Parameters, + ReturnType = void
        internal static string parse(ref ExeBuilder eb, ref int pos)
        {
            MethodData data = new MethodData(); // Should be embedded Type
            var list = eb.list;
            pos++;
            list.assertC("(");
            if (list[pos].Equals("§main")) {
                data.setMain();
                pos++;
                list.assertC(")");
            }
            else data.readSignature(ref eb.list, ref pos);
            // Put the indetn outside
#warning Also handle somehow the §main contents (Maybe own Handler for that (and §program as well))
#warning Called from destFiles, this should just take the lines as they are without parsing (until §endMethod)
            if (!data.isSigOnly()) {
                eb.currentMethod = data;
                var ret = Parser.execRun(ref eb, ref pos, "§endMethod");
                data.setCode(ret);
                pos++; // exec should return on §endMethod, so skip that

                data = eb.currentMethod; // ??
                eb.currentMethod = null;
            }
            eb.AddMethod(data);

            return "";
        }
    }
}
