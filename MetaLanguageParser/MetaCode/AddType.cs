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
        // §addMethod(§main)      /// Adds a EntryMethod
        // §addMethod()(myMethod) /// Adds a method called "myMethod" with no modifiers or Parameters, + ReturnType = void
        internal static string parse(ref ExeBuilder eb, ref int pos)
        {
            TypeData data;// = new TypeData(); // Should be embedded Type
            var list = eb.list;
            pos++;
            list.assertC("(");
            if (list[pos].Equals("§main")) {

                data = TypeData.setMain();
                pos++;
                list.assertC(")");//.assertC("{");
                eb.Indent++;
            } else throw new NotImplementedException("addType.AnythingElse");//data.readSignature(ref eb.list, ref pos);
            eb.currentType = data;

                var ret = Parser.execRun(ref eb, ref pos, "§endType");
                data.setStuff(ret);
                pos++; // exec should return on §endMethod, so skip that

                data = eb.currentType; // ??

            eb.currentType = null;
            eb.AddType(data);
            eb.Indent--;

            return "";
        }
    }
}
