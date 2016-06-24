using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.Parsing
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
            WideGlyph, WChar = WideGlyph,
            WideText, WString = WideText,

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

        /// <summary>
        /// Prints a full TypeName
        /// </summary>
        /// <param name="typeRef"></param>
        /// <returns></returns>
        public string GetTypeOutput(System.CodeDom.CodeTypeReference typeRef)
        {
            string s = String.Empty;

            System.CodeDom.CodeTypeReference baseTypeRef = typeRef;
            while (baseTypeRef.ArrayElementType != null) {
                baseTypeRef = baseTypeRef.ArrayElementType;
            }
            s += GetBaseTypeOutput(baseTypeRef);

            while (typeRef != null && typeRef.ArrayRank > 0) {
                char [] results = new char [typeRef.ArrayRank + 1];
                results[0] = '[';
                results[typeRef.ArrayRank] = ']';
                for (int i = 1; i < typeRef.ArrayRank; i++) {
                    results[i] = ',';
                }
                s += new string(results);
                typeRef = typeRef.ArrayElementType;
            }

            return s;
        }
        // returns the type name without any array declaration.
        private string GetBaseTypeOutput(System.CodeDom.CodeTypeReference typeRef)
        {
            string s = typeRef.BaseType;
            if (s.Length == 0) {
                s = "void";
                return s;
            }

            string lowerCaseString  = s.ToLower( System.Globalization.CultureInfo.InvariantCulture).Trim();
            switch (lowerCaseString) {
                case "system.int16":
                    s = "short";
                    break;
                default:
                    // Make it a globalReference: for(lengthOf BaseType)if '+' or '.', then add baseType/namespace # if '`' then add Generic
                    break;
            }
            return "";
        }
    }

}
