using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser.Parsing
{

    public class MetaType
    {
        /*
        CodeTypeReference = CTRef ArrayElementType, int ArrayRank, string baseType, CTRefOptions Options, CTRefCol TypeArguments
            --> Type / string typeName / CTParam / Type, CTRefOp / strType, CtRefOpt / strType, params CTRef[] typeArgs / strBase, int rank / CTRef arrType, int Rank
            Options --> Enum { Resolve from Global Root // Resolve from TypeArgument }
    */

        UInt16 i = new ushort();
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
            UInt8, UByte = UInt8,
            UInt16, UShort = UInt16,
            UInt32, UInt = UInt32,
            UInt64, ULong = UInt64,
            Float32, Single = Float32,
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

        public static MetaType Factory(string s)
        {
            return new MetaType(resolvedTypes[s.ToLower()]);
        }
        public static MetaType Factory(enumMetaTypes mType)
        {
            return new MetaType(mType);
        }

        private MetaType(enumMetaTypes mType)
        {
            this.metaType = mType;
        }


        public override string ToString()
        {
            switch (metaType) {
                case enumMetaTypes.Invalid:
                    break;
                case enumMetaTypes.Dynamic_Compile:
                    break;
                case enumMetaTypes.Dynamic_Runtime:
                    break;
                case enumMetaTypes.Int8:
                    break;
                case enumMetaTypes.Int16: return "short";
                case enumMetaTypes.Int32: return "int";
                case enumMetaTypes.Int64:
                    break;
                case enumMetaTypes.UInt8:
                    break;
                case enumMetaTypes.UInt16:
                    break;
                case enumMetaTypes.UInt32:
                    break;
                case enumMetaTypes.UInt64:
                    break;
                case enumMetaTypes.Float32:
                    break;
                case enumMetaTypes.Float64:
                    break;
                case enumMetaTypes.Char:
                    break;
                case enumMetaTypes.String: return "String";
                case enumMetaTypes.WChar:
                    break;
                case enumMetaTypes.WString:
                    break;
                case enumMetaTypes.Boolean:
                    break;
                case enumMetaTypes.List:
                    break;
                case enumMetaTypes.Set:
                    break;
                case enumMetaTypes.Map:
                    break;
                case enumMetaTypes.Reference:
                    break;
                default: throw new NotImplementedException("MetaType.ToString(): Unimplemented case " + metaType.ToString());
            }
            throw new NotImplementedException("BLARG of " + metaType);
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


        static Dictionary<string, enumMetaTypes> resolvedTypes = new Dictionary<string, enumMetaTypes>() {
                /// Void
                //{"void", typeof(void) },

                /// Signed Integer
                {"sbyte", enumMetaTypes.Int8 }, {"short", enumMetaTypes.Int16 }, {"int", enumMetaTypes.Int32 }, {"long", enumMetaTypes.Int64 },
                {"int8", enumMetaTypes.Int8 }, {"int16", enumMetaTypes.Int16 }, {"int32", enumMetaTypes.Int32 }, {"int64", enumMetaTypes.Int64 },
                //{"sbyte", typeof(sbyte) }, {"short", typeof(short) }, {"int32", typeof(int) }, {"long", typeof(long) },

                /// Unsigned Integer
                //{"byte", typeof(byte) }, {"ushort", typeof(ushort) }, {"uint", typeof(uint) }, {"ulong", typeof(ulong) },
                //{"byte", typeof(byte) }, {"ushort", typeof(ushort) }, {"UInt32", typeof(uint) }, {"ulong", typeof(ulong) },

                /// Signed decimal number (16 byte)
                //{"decimal", typeof(decimal) },

                /// Floating Point number
               // {"float", typeof(float) }, {"double", typeof(double) },

                /// Boolean
                //{"bool", typeof(bool) },

                /// Character literals
                {"char", enumMetaTypes.Char }, {"string", enumMetaTypes.String },
            };
        
    }

}
