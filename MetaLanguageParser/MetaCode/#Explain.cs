using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaLanguageParser.Parsing;
using static MetaLanguageParser.Parsing.Adapter;

namespace MetaLanguageParser.MetaCode
{
    class Switch // <--- FileName
    {
        /// <summary> Eventual describe the format, like seen in Example</summary>
        /// <example>
        /// §assign(i)(value); 
        /// </example>
        internal static string parse(ref ExeBuilder eb, ref int pos)
        {
			// Your Code
			var list = getList(); // To get the Tokenized List of the Codefile
			/// See Functions in DocXml of that
			
			//CodeBase Handler: All expect pos to be on the first concerning element
			// and they ensure leaving on the element AFTER the last concerning them.
			// The same behaviour is expected from the MetaCode.parse Handler (like this one) 
			CodeBase.readExpression(ref eb, ref pos);
			
			return ""; // If you have nothing to add to the Code
			return stuff; // Likewise, to add it to the Text
		}
	}
}
