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
            this.name = name;
        }

        MetaType type;
        string name;
        bool hasValue;
        object value;

        /// <summary>Whether or not current Type is unsigned. Only applied on Numbers.</summary>
        public bool isUnsigned = false;

        /// <summary>Representing the location/direction: Variable[default], In/Out/Inout, Return</summary>
        public enum enumLocalType
        {
            Variable, In, Out, Inout, Return
        }
        /// <summary>Field holding the location/direction (default: Variable)</summary>
        public enumLocalType dir = enumLocalType.Variable;
    }
}
