using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.Parsing
{
    class LocalData
    {

        public LocalData(MetaType type, string name)
        {
            this.type = type;
            this.Name = name;
        }

        MetaType type;
        public string Name { get; internal set; }
        bool hasValue;
        string value;

        /// <summary>Whether or not current Type is unsigned. Only applied on Numbers.</summary>
        public bool isUnsigned = false;

        /// <summary>Representing the location/direction: Variable[default], In/Out/Inout, Return</summary>
        public enum enumLocalType
        {
            Variable, In, Out, Inout, Return
        }

        internal void setValue(string v)
        {
            value = v;
            hasValue = true;
        }

        /// <summary>Field holding the location/direction (default: Variable)</summary>
        public enumLocalType dir = enumLocalType.Variable;
#warning CUSTOM:: Retrieve them
        public static string forStr_decl = "{0} {1};";
        public static string forStr_def = "{0} {1} = {2};";
        public static string forStr_assign = "{0} = {1};";
        public override string ToString()
        {
#warning Also consider somehow the option "Seperate Decl and Assignment"
            if (hasValue) {
                return string.Format(forStr_def, type, Name, value);
            } else return string.Format(forStr_decl, type, Name);
        }

        internal string getAssign()
        {
            return string.Format(forStr_assign, Name, value);
        }
    }
}
