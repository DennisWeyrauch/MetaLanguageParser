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
        internal static string parse(ref ExeBuilder eb, ref int pos)
        {
            MethodData data = new MethodData(); // Should be embedded Type

            // Isn't that §addMethod / §main here?
            if (eb.list[pos].Equals("§main")) data.setMain();
#warning Also handle somehow the §main contents
#warning Called from destFiles, this should just take the lines as they are without parsing (until §endMethod)
            data.readSignature(ref eb.list, ref pos);
            if (!data.isSigOnly()) {
                eb.currentMethod = data;
                var ret = Parser.execRun(ref eb, ref pos, "§endMethod");
                data.setCode(ret);
                pos++;
                eb.currentMethod = null;
            }
            eb.AddMethod(data);

            return "";
        }
    }
}
