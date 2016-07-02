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
        public LocalData(MetaType type, string name, string v)
        {
            this.type = type;
            this.Name = name;
            this.Value = v;
        }

        MetaType type;
        public string Name { get; internal set; }
        public bool hasValue => Value != null;
        string Value;

        /// <summary>Whether or not current Type is unsigned. Only applied on Numbers.</summary>
        public bool isUnsigned = false;

        /// <summary>Representing the location/direction: Variable[default], In/Out/Inout, Return</summary>
        public enum enumLocalType
        {
            Variable, In, Out, Inout, Return
        }

        /// <summary>Field holding the location/direction (default: Variable)</summary>
        public enumLocalType dir = enumLocalType.Variable;
#warning CUSTOM:: Retrieve them
        static LocalData()
        {
            var dict = Resources.ResourceReader.readAnyFile(Resources.ResxFiles.getLangPath("_vardecl.txt", true));
            forStr_decl = dict["§decl"];
            forStr_assign = dict["§assign"];
            dict.TryGetValue("§define", out forStr_def);
        }
        public static string forStr_decl = "{0} {1};";
        public static string forStr_def = "{0} {1} = {2};";
        public static string forStr_assign = "{0} = {1};";
        public override string ToString()
        {
#warning Also consider somehow the option "Seperate Decl and Assignment"
            if (hasValue) {
                if (forStr_def == null) {
                    return new StringBuilder().AppendFormat(forStr_decl, type, Name).AppendLine().AppendFormat(forStr_assign, Name, Value).ToString();
                } 
                return string.Format(forStr_def, type, Name, Value);
            } else return string.Format(forStr_decl, type, Name);
        }

        internal string getAssign()
        {
            return (hasValue) ? string.Format(forStr_assign, Name, Value) : "";
        }
    }
}
