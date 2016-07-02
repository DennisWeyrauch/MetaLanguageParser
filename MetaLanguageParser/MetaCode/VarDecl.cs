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
            list.assertC("§vardecl").assertC('(');
            /// Multi Declaration
            MetaType type;
            if (list.isCurrent('{')) { //
                // TYPE + // "," + name1 // x? + "}"
                List<LocalData> locals = new List<LocalData>();
                string elem = "";
                do {
                    list.assert('{'); // { -> TYPE
                    type = MetaType.Factory(list.getCurrent());
                    while (list.whileTrue(ref elem, ',')) { // TYPE -> , --or-- NAME -> ,
                        pos++; // , -> NAME
                        locals.Add(new LocalData(type, list[pos]));
                    } // NAME -> }
                    list.assert('}'); // } -> ) -or- } -> ,
                } while (list.isCurrent(',') && list.nextNot(')')); // IF(,) THEN , -> { --or-- , -> ) -> EXCEPTION
                list.assert(')'); // , --> )
                ExeBuilder.AddLocal(locals);
            } else {
                type = MetaType.Factory(list.getCurrent());
                string name = list[++pos];
                list.assertPreInc(')');
                LocalData data;
                if (list.nextIs('=')) {
                    pos++; // = --> {value}
                    var str = CodeBase.readExpression(ref eb, ref pos);
                    data = new LocalData(type, name, str);
                } else {
                    data = new LocalData(type, name);
                }
                eb.currentMethod.addLocal(data);
            }
            //System.Console.ReadLine();
            list.assert(';'); // ; -> {NEXT}



            return "";
        }
        internal static string parse1(ref ExeBuilder eb, ref int pos)
        {
            // §vardecl($$Type$$ $$name$$);
            // §vardecl($$Type$$ $$name$$) = $$value$$;
            var list = eb.list;
            list.assertC("§vardecl").assertC("(");
            MetaType type = MetaType.Factory(list.getCurrent());
            string name = list[++pos];
            list.assertPreInc(")");
            LocalData data;
            if (list.nextIs("=")) {
                pos++; // = --> {value}
                var str = CodeBase.readExpression(ref eb, ref pos);
                data = new LocalData(type, name, str);
            } else {
                data = new LocalData(type, name);
            }
            list.assert(";");


            eb.currentMethod.addLocal(data);

            return "";
        }
    }
}
