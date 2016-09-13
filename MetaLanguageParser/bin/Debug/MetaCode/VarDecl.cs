using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parsing.Adapter;

namespace MetaLanguageParser.MetaCode
{
    class VarDecl
    {
        internal static string parse(ref int pos)
        {
            // §vardecl($$Type$$ $$name$$);
            // §vardecl($$Type$$ $$name$$) = $$value$$;
            // §vardecl({$$Type$$, $$name_n$$, ...});
            // §vardecl({$$Type1$$, ...}, {$$Type2$$, ...}, ...);
            var list = getList();
            list.assertC("§vardecl").assertC('(');
            /// Multi Declaration
            MetaType type;
            if (list.isCurrent('{')) { //
                // TYPE + // "," + name1 // * + "}"
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
                AddLocal(locals);
            } else {
                type = MetaType.Factory(list.getCurrent());
                string name = list[++pos];
                list.assertNext(')');
                LocalData data;
                if (list.nextIs('=')) {
                    pos++; // = --> {value}
                    var str = readExpression();
                    data = new LocalData(type, name, str);
                } else {
                    data = new LocalData(type, name);
                }
                currentMethod.addLocal(data);
            }
            list.assert(';'); // ; -> {NEXT}
            
            return "";
        }
    }
}
