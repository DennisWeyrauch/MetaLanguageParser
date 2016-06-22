using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using enumMetaTypes = MetaLanguageParser.MetaType.enumMetaTypes;

namespace MetaLanguageParser
{
    /// <summary>
    /// An interface to just collect all you'd need for options.
    /// Should later be an simple class with fields/properties containing the values.
    /// 
    /// </summary>
    interface ILangOptions
    {

        /// <summary>Contains the Type representing Int8</summary>
        string Int8 { get; }
        /// <summary>Contains the Type representing Int16</summary>
        string Int16 { get; }
        /// <summary>Contains the Type representing Int32</summary>
        string Int32 { get; }
        /// <summary>Contains the Type representing Int64</summary>
        string Int64 { get; }
        /// <summary>Contains the Type representing Float32</summary>
        string Float32 { get; }
        /// <summary>Contains the Type representing Float64</summary>
        string Float64 { get; }
        /// <summary>Contains the Type representing single characters</summary>
        string Glyph { get; }
        /// <summary>Contains the Type representing Text</summary>
        string Text { get; }
        /// <summary>Contains the Type representing Boolean</summary>
        string Boolean { get; }

        // Collections
        /// <summary>Contains the Type representing List</summary>
        string List { get; }
        /// <summary>Contains the Type representing Set</summary>
        string Set { get; }
        /// <summary>Contains the Type representing Map</summary>
        string Map { get; }
        

        /// <summary>Contains the Type representing Compiletime Dynamics</summary>
        string Dynamic_Compile { get; }
        /// <summary>Contains the Type representing Runtime Dynamics (Duck Typing)</summary>
        string Dynamic_Runtime { get; }

        string Reference { get; }


        /// <summary>Contains the value for an empty Reference</summary>
        string Null { get; }
        /// <summary>Contains the value representing boolean Truth</summary>
        string True { get; }
        /// <summary>Contains the value representing boolean Untruth</summary>
        string False { get; }


        string Ref_This { get; }
        string Base(string args);
        //string Super(string args);
        //string Default(string args);
        //string NameOf(string args);
        //string TypeOf(string args);

        /// <summary>
        /// Make an expression that returns the size of the given string
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        string SizeOf(string args);



    }

    public abstract class LanguageOptions
    {

        #region DictProperties
        // Search:	\t([^\t, ]*)( = \w*)?,
        // Replace:	public string ${1} \{ get, private set \};
        // Replace:	/// <summary>Contains the Type representing ${1}</summary>\r\n\t\tpublic string ${1} \{ get, private set \};
        // Replace:	/// <summary>Contains the Type representing ${1}</summary>\r\n\t\tpublic string ${1} => metaDict[${1}];

        // Search:	public string ([^ ]*) => metaDict\[enumMetaTypes\.\1\];
        // Replace:	\t{enumMetaTypes.${1}, ""},
        // Replace:	metaDict.Add\(enumMetaTypes.${1}, readLine(X), true\);
        public RWDictionary<enumMetaTypes, string> metaDict = new RWDictionary<enumMetaTypes, string>() {
            {enumMetaTypes.Int8, ""},
            {enumMetaTypes.Int16, ""},
            {enumMetaTypes.Int32, ""},
            {enumMetaTypes.Int64, ""},
            {enumMetaTypes.Float32, ""},
            {enumMetaTypes.Float64, ""},
            {enumMetaTypes.Glyph, ""},
            {enumMetaTypes.Text, ""},
            {enumMetaTypes.Boolean, ""},
            {enumMetaTypes.List, ""},
            {enumMetaTypes.Set, ""},
            {enumMetaTypes.Map, ""},
            {enumMetaTypes.Reference, ""},
            {enumMetaTypes.Dynamic_Compile, ""},
            {enumMetaTypes.Dynamic_Runtime, ""},
        };

        /*public RWDictionary<enumMetaRef, string> metaRef = new RWDictionary<enumMetaRef, string>() {
			// This
			// Base
		};//*/

        /// <summary>Contains the Type representing Int8</summary>
        public string Int8 => metaDict[enumMetaTypes.Int8];
        /// <summary>Contains the Type representing Int16</summary>
        public string Int16 => metaDict[enumMetaTypes.Int16];
        /// <summary>Contains the Type representing Int32</summary>
        public string Int32 => metaDict[enumMetaTypes.Int32];
        /// <summary>Contains the Type representing Int64</summary>
        public string Int64 => metaDict[enumMetaTypes.Int64];
        /// <summary>Contains the Type representing Float32</summary>
        public string Float32 => metaDict[enumMetaTypes.Float32];
        /// <summary>Contains the Type representing Float64</summary>
        public string Float64 => metaDict[enumMetaTypes.Float64];
        /// <summary>Contains the Type representing single characters</summary>
        public string Glyph => metaDict[enumMetaTypes.Glyph];
        /// <summary>Contains the Type representing Text</summary>
        public string Text => metaDict[enumMetaTypes.Text];
        /// <summary>Contains the Type representing Boolean</summary>
        public string Boolean => metaDict[enumMetaTypes.Boolean];

        // Collections
        /// <summary>Contains the Type representing List</summary>
        public string List => metaDict[enumMetaTypes.List];
        /// <summary>Contains the Type representing Set</summary>
        public string Set => metaDict[enumMetaTypes.Set];
        /// <summary>Contains the Type representing Map</summary>
        public string Map => metaDict[enumMetaTypes.Map];
        /// <summary>Contains the Type representing Map</summary>
        //public string makeArray(MetaType mt);

        /// <summary>Contains the Type representing the Root object type </summary>
        public string Reference => metaDict[enumMetaTypes.Reference];

        /// <summary>Contains the Type representing Compiletime Dynamics</summary>
        public string Dynamic_Compile => metaDict[enumMetaTypes.Dynamic_Compile];
        /// <summary>Contains the Type representing Runtime Dynamics (Duck Typing)</summary>
        public string Dynamic_Runtime => metaDict[enumMetaTypes.Dynamic_Runtime];


        #endregion

        #region DemoStuff
        private string fillDict_demo()
        {
            object X = null;
            metaDict.Add(enumMetaTypes.Int8, readLine(X), true);
            metaDict.Add(enumMetaTypes.Int16, readLine(X), true);
            metaDict.Add(enumMetaTypes.Int32, readLine(X), true);
            metaDict.Add(enumMetaTypes.Int64, readLine(X), true);
            metaDict.Add(enumMetaTypes.Float32, readLine(X), true);
            metaDict.Add(enumMetaTypes.Float64, readLine(X), true);
            metaDict.Add(enumMetaTypes.Glyph, readLine(X), true);
            metaDict.Add(enumMetaTypes.Text, readLine(X), true);
            metaDict.Add(enumMetaTypes.Boolean, readLine(X), true);
            metaDict.Add(enumMetaTypes.List, readLine(X), true);
            metaDict.Add(enumMetaTypes.Set, readLine(X), true);
            metaDict.Add(enumMetaTypes.Map, readLine(X), true);
            metaDict.Add(enumMetaTypes.Reference, readLine(X), true);
            metaDict.Add(enumMetaTypes.Dynamic_Compile, readLine(X), true);
            metaDict.Add(enumMetaTypes.Dynamic_Runtime, readLine(X), true);
            return null;
        }

        private string readLine(object x)
        {
            throw new NotImplementedException();
        }


        // References & Literals
        /// <summary>Contains the Type for an empty Reference</summary>
        public string Null => "null";//metaDict[enumMetaTypes.Null];
                                     /// <summary>Contains the Type representing boolean Truth</summary>
        public string True => "true";//metaDict[enumMetaTypes.True];
                                     /// <summary>Contains the Type representing boolean Untruth</summary>
        public string False => "false";//metaDict[enumMetaTypes.False];
        public string This => "this";
        public string Base(string args) => "base(" + args + ")";
        public string Super(string args) => Base(args);
        public string Default(string args) => "default(" + args + ")";
        public string NameOf(string args) => "nameof(" + args + ")";
        public string TypeOf(string args) => "typeof(" + args + ")";
        public string SizeOf(string args) => "sizeof(" + args + ")";

        #endregion

        string _langName = "";
        string _suffix = "";
        public string LangName => _langName;
        public string Suffix => _suffix;

        //public abstract LanguageOptions(string name, string suffix);
        public abstract string fillDict();

        public abstract string makeField();
        public abstract string makeMethod();
        public abstract string makeProperty();
        // printGenerics


        // Maybe MemberBuilder?
        // Likely reading SrcCode, compile and generate BinData into same folder(but+ /bin)
        // Also, first look into the bin folder before recompile src.
        /*
		enum
		valueType / struct (Has own option?)
		
		isOOP
		getStatementSeperator
		getBlock_Open
		getBlock_Close
		hasSyntacticWhitespace
		
		Flag-Options are true just cause they exist(so "isOOP" on own Line sets OOPFlag to true)
			Begin/Stop with "$$>Begin" or such
		Also, reading in TypeNames with
			"ENUMNAME := TypeName"
			"unsigned ENUMNAME := TypeName"
			"list := System.Collections.Generic.List"
			$>	isGeneric := 1
		
		//*/

    }
}
