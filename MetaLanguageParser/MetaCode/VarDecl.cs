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
            LocalData data = new LocalData(null, "");
            /*
            data.readSignature(ref eb.list, ref pos); // Read Type + Name
            if (!data.isSigOnly()) { // If next not ;
                // assert '='
                // Parser.readExpression(ref...)
                add as Value    

            }//*/
            eb.currentMethod.addLocal(data);

            return "";
        }
    }
}
