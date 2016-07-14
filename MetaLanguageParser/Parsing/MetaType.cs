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
            /// <summary>Signed Integer</summary>
            Int8, Int16, Int32, Int64,
            /// <summary>Unsigned Integer</summary>
            UInt8, UInt16, UInt32, UInt64,
            /// <summary>Floating Point</summary>
            Float32, Float64, Float128,
            /// <summary>Text</summary>
            Char, Text, String = Text,
            WChar, WString,

            Boolean, Bool = Boolean,
            /// <summary>Represents an ordered collection.</summary>
            List,
            /// <summary>Represents an unordered collection.</summary>
            Set,
            /// <summary>Represents an Key-Value collection</summary>
            Map,
            /// <summary>Represents an Reference. Use ObjectType</summary>
            Reference, Object = Reference,
            /// <summary>Self set Type</summary>
            Any
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
                if (metaType != enumMetaTypes.Reference && metaType != enumMetaTypes.Any) throw new InvalidOperationException();
                return _objName;
            }
            set { _objName = value; }
        }

        public static MetaType Factory(string s)
        {
            if(resolvedTypes.ContainsKey(s))
               return new MetaType(resolvedTypes[s.ToLower()]);
            else return new MetaType(s);
        }
        public static MetaType Factory(enumMetaTypes mType)
        {
            return new MetaType(mType);
        }

        private MetaType(enumMetaTypes mType)
        {
            this.metaType = mType;
        }
        private MetaType(string s)
        {
            this._objName = s;
            this.metaType = enumMetaTypes.Any;
        }


        static MetaType()
        {
            //ResourceReaderreadAnyFile(getLangPath("_types.txt"), true));
        }

        public static void setTypeDict(Dictionary<string, string> dict) => typeDict = dict;
        static Dictionary<string,string> typeDict;// = new Dictionary<string,string>();

        string getTypeKey(string str)
        {

            switch (metaType) {
                case enumMetaTypes.Dynamic_Compile:
                case enumMetaTypes.Dynamic_Runtime:
                    //case enumMetaTypes.List:
                    //case enumMetaTypes.Set:
                    //case enumMetaTypes.Map:
                    throw new NotImplementedException("BLARG of " + metaType);
                    break;
                case enumMetaTypes.Any:
                    return ObjectType;
                default:
                    return metaType.ToString();
            }
        }

        public override string ToString()
        {
            /**switch (metaType) {
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
            //*/

            string val,key = "";
            switch (metaType) {
                case enumMetaTypes.Dynamic_Compile:
                case enumMetaTypes.Dynamic_Runtime:
                //case enumMetaTypes.List:
                //case enumMetaTypes.Set:
                //case enumMetaTypes.Map:
                    throw new NotImplementedException("BLARG of " + metaType);
                    break;
                case enumMetaTypes.Any:
                    key = ObjectType;
                    break;
                default:
                    key = metaType.ToString();
                    break;
            }
            if (!typeDict.TryGetValue(key.ToLower(), out val)) val = "_MISSING_"+key;
            // Add Array and shit
            return val;
        }

        void __Test1(string str)
        {
            string key = "";
            if (typeDict.TryGetValue(str, out key)) ; else key = str;
        }
        void __Test2(string str)
        {
            string key = "";
            if (!typeDict.TryGetValue(str, out key)) key = str;
        }
        void __Test3(string str)
        {  
            string key = (typeDict.ContainsKey(str))? typeDict[str] : str;
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

        /// <summary>
        /// Dictionary to unify the basic datatypes into a common root (the Enum)
        /// </summary>
        static Dictionary<string, enumMetaTypes> resolvedTypes = new Dictionary<string, enumMetaTypes>() {
                /// Void
                //{"void", typeof(void) },

                /// Signed Integer
                {"sbyte", enumMetaTypes.Int8 }, {"short", enumMetaTypes.Int16 }, {"int", enumMetaTypes.Int32 }, {"long", enumMetaTypes.Int64 },
                {"int8", enumMetaTypes.Int8 }, {"int16", enumMetaTypes.Int16 }, {"int32", enumMetaTypes.Int32 }, {"int64", enumMetaTypes.Int64 },
                //{"sbyte", typeof(sbyte) }, {"short", typeof(short) }, {"int32", typeof(int) }, {"long", typeof(long) },

                /// Unsigned Integer
                {"byte", enumMetaTypes.UInt8 }, {"ushort", enumMetaTypes.UInt16 }, {"uint", enumMetaTypes.UInt32 }, {"ulong", enumMetaTypes.UInt64 },
                {"uint8", enumMetaTypes.UInt8 }, {"uint16", enumMetaTypes.UInt16 }, {"uint32", enumMetaTypes.UInt32 }, {"uint64", enumMetaTypes.UInt64 },


                /// Floating Point number
                {"float32", enumMetaTypes.Float32 },{"float", enumMetaTypes.Float32 }, {"single", enumMetaTypes.Float64 },
                {"float64", enumMetaTypes.Float64 },{"double", enumMetaTypes.Float64 },
                /// Signed decimal number (16 byte)
                {"decimal", enumMetaTypes.Float128 },{"float128", enumMetaTypes.Float128 },

                /// Boolean
                {"bool", enumMetaTypes.Bool}, {"boolean", enumMetaTypes.Bool},

                /// Character literals
                {"char", enumMetaTypes.Char }, {"string", enumMetaTypes.String },
            };
        
    }

}
