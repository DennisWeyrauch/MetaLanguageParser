using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Common;
using StringBuilder = System.Text.StringBuilder;
using MemberType = System.Reflection.MemberTypes;
using System.CodeDom.Compiler;
using MetaLanguageParser;
using MetaLanguageParser.MetaCode;
using System.IO;
using static MetaLanguageParser.Resources.ResxFiles;
using MetaLanguageParser.Parsing;

namespace MetaLanguageParser
{

    /// <summary>Delegate to use for Keyword-Functions.<para/>
    /// eb = <see cref="ExeBuilder"/>-Reference. ##
    /// pos = Current position in Token-List.</summary>
    /// <param name="eb"><see cref="ExeBuilder"/>-Reference.</param>
    /// <param name="pos">Current position in Token-List.</param>
    /// <returns>Position to continue with.</returns>
    public delegate string FuncDel(ref ExeBuilder eb, ref int pos);

    /// <summary>Data class holding all relevant data for the parser.</summary>
    public class ExeBuilder {
        #region Fields

        /// <summary>Current list of tokenized code.</summary>
        public ListWalker list;
        //list.GetEnumerator(); // Just to see it
        public static Dictionary<string,FuncDel> kw { get; private set; }
        public IndentedTextWriter writer;
        public int Indent = 0;


        /** Needs the following:
         -- List of Usings
         -- Declared Types, containing
            -- Scope, evt. Modifier, Name, Classification (class, struct, interface, enum, ...)
            -- Namespace Name
            -- List of Fields
                (Scope, Modifier, Type, Name, hasInit, initval)
            -- List of Methods
                Scope, Modifier, retType, Name, List<Args>, List<Loc>, Code
                Optional Specifics: Generics + "where T is ...", Managed Exceptions, Assertions
            -- etc.
        
        //*/

        /// <summary> string: nameToken as found ## Type: Type object representing the token.</summary>
        public Dictionary<string,Type> resolvedTypes;
        List<string> usings;
        
        string _typeName;
        public void invalidateTypeName() { _typeName = "<invalid-global-code>"; }
        
        /// <summary>To validate whether or not outParameter exist and they've been assigned to. Before a method transfers control, it checks if all members are set to true, and throws Exception otherwise. <para/>
        /// For convenience, any locals that are assigned to are added with true, while their absense says they're uninitialized.
        ///</summary>
        internal Dictionary<string, MetaType> args; // Required: Position
        /// <summary>NAME -- Type</summary>
        public Dictionary<string, MetaType> locals;


        // Name, MemberType, Reference;  last is null for overloads
        /// <summary>Dictionary holding references to all members.<para/>
        /// From Builder to callable Info: memberDict[name, memType].GetBaseDefinition(); // At least for methods</summary>
        /// <example>methDict["Main"].GetBaseDefinition();</example>
        public Dictionary<string, MemberType, MemberInfo> memberDict;
        /// <summary>
        /// Method-Dictionary with the arguments "MethodName", "ParameterTypes", "MethodInfo". Used for overload differentiation.
        /// </summary>
        public Dictionary<string, Type[], MethodInfo> methDict2;
        public Dictionary<string, string, MethodInfo> methDict_sig;
        public Dictionary<string, MethodData> methDict = new Dictionary<string, MethodData>();
        #endregion
        #region Constructors


        private ExeBuilder(List<string> l)
        {
            list = (ListWalker) l;
            resolvedTypes = new Dictionary<string, Type>() {
                // Void
                {"void", typeof(void) },
                // Signed Integer
                {"sbyte", typeof(sbyte) }, {"short", typeof(short) }, {"int", typeof(int) }, {"long", typeof(long) },
                // Unsigned Integer
                {"byte", typeof(byte) }, {"ushort", typeof(ushort) }, {"uint", typeof(uint) }, {"ulong", typeof(ulong) },
                // Signed decimal number (16 byte)
                {"decimal", typeof(decimal) },
                // Floating Point number
                {"float", typeof(float) }, {"double", typeof(double) },
                // Boolean
                {"bool", typeof(bool) },
                // Character literals
                {"char", typeof(char) }, {"string", typeof(string) },
            };

             usings = new List<string>();
        }
        
        public ExeBuilder(ListWalker lw, string langCode)//, IndentedTextWriter write)
        {
            list = lw;
            LangCode = langCode;
            _instance = this;
        }
        

        static ExeBuilder _instance;
        internal static ExeBuilder Instance => _instance ?? (_instance = new ExeBuilder(null));
        internal static ExeBuilder getInstance() => _instance ?? (_instance = new ExeBuilder(null));
        #endregion

        #region Builder-Functions

        /// <summary>
        /// Checks if Type already contains a member with the same name. Throws exception if true, else adds it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="memType"></param>
        /// <param name="member"></param>
        internal void addMember(string name, MemberType memType, MemberInfo member)
        {
            //if (memberDict.ContainsKey(name)) throw new InvalidSyntaxException(CS0102, list.Index, _typeName, name);
            memberDict.Add(name, memType, member);
        }
        
        void registerBasicDataTypes(){
            /// Fill the dictionary
            // Also fill in Object, Null, Collections (with full path)
        }

        public MethodData currentMethod { get; set; }
        internal void AddMethod(MethodData data)
        {
            methDict.Add(data.Name, data);
        }
        
        #endregion

        #region Scope
        internal Stack<CodeBase> codeBase = new Stack<CodeBase>();
        #endregion

        /// <summary>
        /// Finds out if the given element exists as local, argument or member reference
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool isNameFree(string name)
        {
            //string isNameFree = ""; // MethodName is ok.
            //string ExeBuilder = ""; // Typename is ok.
            //string isNameFree = ""; // CS0128 because local
            //string name = ""; // CS0136 because enclosing local or parameter
            //if (test == 5) ; // CS0841 -- Using local before declared.
            //if (test == 5) ; // CS0844 -- Using local before declared. Hides Field 'TYPE.test' (But not Props)
            //if (test == 5) ; // CS0103 -- name 'test' does not exist
            //string this = ""; // CS1001 -> Identifier, CS1604 -> Can't assign to 'this', CS1002 -> ;
            /*if (name.Equals("this")) {
                string[] formats, args;
                if (method.IsStatic) {
                    formats = new string[4];
                    args = new string[4];
                } else {
                    formats = new string[3];
                    args = new string[3];
                }
                //if (method.IsStatic) args = formats = new string[4];
                //else args = formats = new string[3];
                var prev = list[list.Index-1];
                var next = list[list.Index+1];
                // if var: CS0103"var"  else: CS1001
                if (prev.Equals("var")) { formats[0] = CS0103; args[0] = prev; }
                else { formats[0] = CS1001; }
                // if next'=' CS1604 else CS0201
                if (next.Equals("=")) { formats[1] = CS1604; args[1] = "this"; }
                else { formats[1] = CS0201; }
                // CS1002
                if (method.IsStatic) {
                    formats[2] = CS0026;
                    formats[3] = CS1002;
                } else formats[2] = CS1002;
                InvalidSyntaxException.ThrowMultiple(formats, list.Index, args);
            }//*/
            //bool isFree = locals.ContainsKey(name) || args.ContainsKey(name);

            return true;
        }

    }
}