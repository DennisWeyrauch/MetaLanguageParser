using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser
{

    public class MetaType
    {
        public enum enumMetaTypes
        {
            /// <summary>Not yet defined type</summary>
            Invalid,
            /// <summary>Dynamic reference (Typeinference from use / assignment)</summary>
            Dynamic_Compile,
            /// <summary>Dynamic reference (Duck typing)</summary>
            Dynamic_Runtime,
            /// <summary>1 byte integer</summary>
            Int8, Byte = Int8,
            /// <summary>2 byte integer</summary>
            Int16, Short = Int16,
            /// <summary>4 byte integer</summary>
            Int32, Int = Int32,
            /// <summary></summary>
            Int64, Long = Int64,
            Float32, Float = Float32,
            Float64, Double = Float64,
            Glyph, Char = Glyph,
            Text, String = Text,

            Boolean, Bool = Boolean,
            /// <summary>Represents an ordered collection.</summary>
            List,
            /// <summary>Represents an unordered collection.</summary>
            Set,
            /// <summary>Represents an Key-Value collection</summary>
            Map,
            /// <summary>Represents an Reference. Use ObjectType</summary>
            Reference, Object = Reference,
        }
        public enumMetaTypes metaType = enumMetaTypes.Invalid;
        /// <summary>
        /// Contains Null, Object, DBNull, Bool, Char, int8,16,32,64+Float32/64 in signed/unsigned, Decimal, DateTime, String
        /// </summary>
        public TypeCode primitive = TypeCode.Object;

        /// <summary>Representing the location/direction: Variable[default], In/Out/Inout, Return</summary>
        public enum enumLocalType
        {
            Variable, In, Out, Inout, Return
        }
        /// <summary>Field holding the location/direction (default: Variable)</summary>
        public enumLocalType dir = enumLocalType.Variable;

        /// <summary>Whether or not current Type is unsigned. Only applied on Numbers.</summary>
        public bool isUnsigned = false;

        /// <summary>Flag to determine if this is an array</summary>
        public bool isArray = false;

        // <summary>Whether or not the field was initialized</summary>
        //public bool isDefault = false;

        public bool isGeneric = false;
        public MetaType generics = null;
        public MetaType generics2 = null;

        /// <summary>Field for the ObjectType Property.</summary>
        private string _objName = "";
        /// <summary>Contains the name of the type this MetaType represents</summary>
        /// <exception name="InvalidOperationException">When <ref name="metaType"/> is not enumMetaTypes.Reference</exception>
        public string ObjectType
        {
            get
            {
                if (metaType != enumMetaTypes.Reference) throw new InvalidOperationException();
                return _objName;
            }
            set { _objName = value; }
        }
    }

}
