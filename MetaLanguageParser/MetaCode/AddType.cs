using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parsing.Adapter;

namespace MetaLanguageParser.MetaCode
{
    class AddType
    {
        /// <summary>
        /// §addType(§main)  ---    /// Adds a EntryType like in "/myLang/§mainType.{suffix}"
        /// </summary>
        internal static string parse(ref int pos)
        {
            TypeData data;
            var list = getList();
            pos++;
            list.assertC('('); // Skip §addMethod
            if (list[pos].Equals("§main")) {
                data = TypeData.setMain();
                pos++;
            } else readType(out data, ref pos);
            list.assertC(')');
            currentType = data;

            var ret = execRun("§endType");
            data.setStuff(ret);
            pos++; // exec should return on §endType, so skip that
            
            AddType(data);
            currentType = null;

            return "";
        }
		
		// public static class NAME<?> ext SUPER1, SUPER2 impl i1, i2 
		static void readType(out TypeData data, ref int pos){
            var list = getList();
			//throw new NotImplementedException("addType.AnythingElse");
			// public, static
			var mods = new Stack<string>();
			while(KEYWORD.modDict.ContainsKey(list[pos])){
                mods.Push(list[pos]); // Push onto Stack
				pos++;
			} // Default modifiers when nothing was set is determined by LangFiles (extra Section)
			/// class, interface, struct, enum
			/*data.setMode(list[pos]); // First Argument
			pos++;
			data.setName(list[pos]); // Second Argument
			pos++;
            //*/
            data = new TypeData(TypeData.getModeEnum(list[pos++]), list[pos++]);
			data.setMods(mods);

			// ...Generics... //
			//pos++;
			/// SuperClass ///
			/*if(list[pos].Equals(KEYWORD.Extend)){
				pos++;
				while(!list[pos].Equals(SYMBOL.Block, KEYWORD.Implement)){
					data.pushSuper(list[pos]); // read full Type, with eventual Generics
					pos++;
					if(list[pos].Equals(SYMBOL.ListSeperator)){
						pos++;
						if(!OPTION.MultiSuper) throw new NotSupportedException("MultiSuper not supported."); // Or ask, ignore, etc.
					}
				}
			}
			/// Interfaces ///
			if(list[pos].Equals(KEYWORD.Implement)){
				// OPTION.Interfaces ?
				pos++;
				while(!list[pos].Equals(SYMBOL.Block)){
					data.pushInterface(list[pos]); // read full Type, with eventuall Generics
					pos++;
					if(list[pos].Equals(SYMBOL.ListSeperator)) pos++;
				}
			}//*/
			/// Code Part ///
			// should leave on ')'
		}
    }
}
