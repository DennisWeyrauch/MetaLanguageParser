using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.MetaCode
{
    class Switch
    {
        /// <summary>
        /// §assign(i)(value);
        /// </summary>
        internal static string parse(ref ExeBuilder eb, ref int pos)
        {
            var list = eb.list;
			#warning __FIX Compile DocXML
            list.assertC("§switch").assertC("(");
			//list[pos++];
            string filter = CodeBase.readExpression(ref eb, ref pos); // Filter
            list.assertC(")").assertC("{");
			#warning __FIX Conditional
			while(true){ // Until Balanced '}'
				CaseEntry entry = new CaseEntry(); // Assigne in case
				string keyword = list[pos];
				switch(keyword){
					/// case $$filter$$: §inc§n $$code::readStatements$$ §dec§n
					case "case":
						break;
					/// multicase (X,Y,Z,...): §inc§n $$code::readStatements$$ §dec§n
					case "multicase":
						entry.list = new List<CaseEntry>();
						// .add(...)
						break;
					/// default: §inc§n $$code::readStatements$$ §dec§n
					case "default":
						break;
				}
				// Read the code, assign to case
				entry.Code = CodeBase.readExpression(ref eb, ref pos);
				switch(keyword){
					case "case":      /* Add To listCases */ break;
					case "multicase": /* Add To listMulti */ break;
					case "default":
						if(entryDefault == null) entryDefault = entry;
						else throw new InvalidSyntaxException("Multiple Defaults");
						break;
				}
			} // Thought about it: Allow Default anywhere (but only once)
			
            list.assertC("}");
			#warning __IDEA Add hook to check if given file exists in /_lang ?
			#warning __IDEA And another to read in files
				//(Maybe just a class containing all these hooks?) Incl. CodeBase Stuff
			
			/* Locals used below
			filter // Contains Switch Filter Expression
			listCases // List of "case"
			listMulti // List of "multicase"-Lists
			entryDefault // Contains Default Entry
			New:
				name <-- Contains VarName of Filter
				entry, multi <-- Foreach 
				i <-- For Counter
				len <-- length of multiList
			Functions: getNewLocal("") -- Checks if Local exists, and returns +1 if it does
				§OrElse / §AndAlso: Chained Or / And Conditionals
				§break: Just a new KeyWordFile
			Also move them into Proper Adapter Class
			//*/
			#region Building Code
			/**
			if switch exists, then readin, and apply the format strings
				if multicase is missing, then do foreach() apply case
				Throw error when switch/case/default missing? Or just treat like file does not exist?
			If File does not exist: //*//**/
				// Note: NL => Add Newline + Entract/Retract Indent (Indented StreamWriter)
				// Actually... fuck newline. Tokenizer removes them anyway
				// On the otherside.... Useful for error Printing
				string name = getNewLocal("filter");
				add($"§vardecl(Any {name}) = {filter};").NL
				add("while(true){").NL // While so that you don't have to remove breaks
				foreach(entry in listCases){
					.add($"if (== {name} {entry.Filter}) {").NL.add(entry.Code).NL.add("} else ")
				}
				if(!listMulti.IsEmpty){
					foreach(entry in listMulti){
						var len = entry.list.length
						.add($"if (§OrElse(")
						.add($"(== {name} {entry.list[0].Filter})")
						//foreach(var multi in entry.list) .add($"(== {name} {multi.Filter}),")
						for(int i=1;i<len;i++) .add($",(== {name} {entry.list[i].Filter})")
						.add(")) {").NL.add(entry.Code).NL.add("} else ")
					}
				}
				if(entryDefault.IsEmpty) add("{} §break ").NL
				else add("{").NL.Add(entryDefault.Code).NL.add("}")
				
				// Close off again
				NL.add(" §break }").NL
			//*/
			// Then somehow trying to get that reparsed.
			#warning __ADD Add hook for TokenizeCode(TextWriter.toString)
			/// Already finished string: string result = ReparseCode(TextWriter.toString())
				/// which handles the Tokenizer, ListWalker, and the like without need to explain that
			#endregion
            return string.Format(LocalData.forStr_assign, name, value);
        }
		class CaseEntry {
			public string Filter;
			public List<CaseEntry> list;
			public string Code;
		}
    }
}
