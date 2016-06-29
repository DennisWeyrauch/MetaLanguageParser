using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.MetaCode
{
    class VarDecl// : ICode
    {
        internal static string parse(ref ExeBuilder eb, ref int pos)
        {
            // §vardecl($$Type$$ $$name$$);
            // §vardecl($$Type$$ $$name$$) = $$value$$;
            var list = eb.list;
            list.assertC("§vardecl").assertC("(");
            MetaType type = MetaType.Factory(list.getCurrent());
            //list.assertC(")").assertC("(");
            string name = list[++pos];
            list.assertPreInc(")");
            LocalData data = new LocalData(type, name);
            if (list.nextIs("=")) {
                pos++; // = --> {value}
                data.setValue(CodeBase.readExpression(ref eb, ref pos));
            }
            list.assert(";");


            eb.currentMethod.addLocal(data);

            return "";
        }
    }
}
