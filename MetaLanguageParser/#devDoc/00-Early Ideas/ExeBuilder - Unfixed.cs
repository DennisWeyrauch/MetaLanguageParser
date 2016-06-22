using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ILExtensions;
using Common;
using static Parser.ResourceNames;
using StringBuilder = System.Text.StringBuilder;
using MemberType = System.Reflection.MemberTypes;

namespace Parser
{
    /// <summary>
    /// This enum is for determining the ProgramFlow, and which Keywords are currently allowed.
    /// </summary>
    [Flags]
    public enum Scope
    {
        /// <summary>This is the empty scope. An <see cref="ExeBuilder"/>'s scope is initialized with this value.</summary>
        Assembly = 0,
        /// <summary>Code is in a Namespace, and imports are allowed.</summary>
        Namespace,
        /// <summary>Code is in a Namespace, but imports are not allowed anymore.</summary>
        NoImports,
        Mask_NS,// = Assembly | Namespace | NoImports,
        /// <summary>[ 8*]Code defines a new Type (class, struct, interface, enum, delegate). Also sevres as bitmask for these Types.</summary>
        /// <remarks>Also, to reduce ScopeLists that allow new types (which are either all or none.)</remarks>
        //TypeDeclaration = 4,
        Type = 8,
        /// <summary>[ 8] Code is in the body of a TypeDefinition, where methods and fields are allowed.</summary>
        Class = Type + 1,
        /// <summary>[ 9] Code is in the body of an abstract class, which allows abstract members.</summary>
        AbstractClass  = Type + 2,
        /// <summary>[10] Code is in the body of a static class, which disallows inheritance, instance and .ctor.</summary>
        /// <remarks>But nested non-static is allowed, which in turn can contain these things</remarks>
        StaticClass = Type + 3,
        /// <summary>[11] </summary>
        Struct = Type + 4,
        /// <summary>[12] Code is in an Interface, which makes all members implicit public and abstract. </summary>
        Interface = Type + 5,
        /// <summary>[13] Code is in an <see cref="System.Enum"/>, an user-defined <see cref="ValueType"/></summary>
        Enum = Type + 6,
        /// <summary>[16*] Code defines a new method, constructor, or accessor of a Property, Indexer, or event.</summary>
        Method = 16,
        /// <summary>[17*] Scope to ease ErrorParser. Prevents new Scope. Not useful for anything else.</summary>
        AbstractMethod = Method + 1,
        /// <summary>[18] Code defines a new Property, a pair of methods accessing an internal (or specified) field.</summary>
        Property = Method+2,
        /// <summary>[32] Code emits a <see cref="System.Reflection.MethodBody"/>, which allows Flowcontrol-structures. (includes ConstructorBody)</summary>
        MethodBody = Method*2,
        /// <summary>[33] Code emits the <see cref="System.Reflection.MethodBody"/> of a constructor, which prevents the use of return values.</summary>
        ConstructorBody = MethodBody+1,
        /// <summary>[64] Code is currently in a nested scope inside a <see cref="MethodBody"/>.</summary>
        Nested = MethodBody*2,
		//[128*] Temporary common scope for MemberBuilder.
        Member = Nested*2 // Maybe as common for Non-Type Members? (Don't think so)
    }
    
	public MetaType{
		public enum enumMetaTypes
		{
			/// <summary>Not yet defined type</summary>
			Invalid,
			/// <summary>Dynamic reference (Typeinference from use / assignment)</summary>
			Dynamic_Compile,
			/// <summary>Dynamic reference (Duck typing)</summary>
			Dynamic_Runtime,
			/// <summary></summary>
			Int8 = Byte,
			Int16 = Short,
			Int32 = Int,
			Int64 = Long,
			Float32 = Float,
			Float64 = Double,
			Glyph = Char,
			Text = String,
			Boolean = Bool,
			/// <summary>Represents an ordered collection.</summary>
			List,
			/// <summary>Represents an unordered collection.</summary>
			Set,
			/// <summary>Represents an Key-Value collection</summary>
			Map,
			/// <summary>Represents an Reference. Use ObjectType</summary>
			Reference = Object,
		}
		public enumMetaTypes metaType = enumMetaTypes.Invalid;
		
		/// <summary>Representing the location/direction: Variable[default], In/Out/Inout, Return</summary>
		public enum enumLocalType {
			Variable, In, Out, Inout, Return
		}
		/// <summary>Field holding the location/direction (default: Variable)</summary>
		public enumLocalType dir = enumLocalType.Variable;
		
		public bool isUnsigned = false;
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
		public string ObjectType {
			get {
				if(metaType != enumMetaTypes.Reference) throw new InvalidOperationException();
				return _objName;
			}
			set { _objName = value; }
		}
	}
	/// <summary>Data class holding all relevant data for the parser.</summary>
    public class ExeBuilder {
        #region Fields
        // Add current ModuleBuilder, AssemblyBuilder, TypeBuilder, etc.
        // Also contains addType, addMethod etc. which all return their respective Builders.
        /// <summary>Current list of tokenized code.</summary>
        public ListWalker list;
        //list.GetEnumerator(); // Just to see it
        public static Dictionary<string,Keyword.del> kw { get; private set; }

        /// <summary> string: nameToken as found ## Type: Type object representing the token.</summary>
        public Dictionary<string,Type> resolvedTypes;
        List<string> usings;

        public string filename { get; private set; }
        
        public void invalidTypeName() { _typeName = "<invalid-global-code>"; }
        
        /// <summary>To validate whether or not outParameter exist and they've been assigned to. Before a method transfers control, it checks if all members are set to true, and throws Exception otherwise. <para/>
        /// For convenience, any locals that are assigned to are added with true, while their absense says they're uninitialized.
        ///</summary>
        internal Dictionary<string, MetaType> Arguments; // Required: Position
		/// <summary>NAME -- Type</summary>
        public Dictionary<string, MetaType> Locals;


        // Name, MemberType, Reference;  last is null for overloads
        /// <summary>Dictionary holding references to all members.<para/>
        /// From Builder to callable Info: memberDict[name, memType].GetBaseDefinition(); // At least for methods</summary>
        /// <example>methDict["Main"].GetBaseDefinition();</example>
        public Dictionary<string, MemberType, MemberInfo> memberDict;
        /// <summary>
        /// Method-Dictionary with the arguments "MethodName", "ParameterTypes", "MethodInfo". Used for overload differentiation.
        /// </summary>
        public Dictionary<string, Type[], MethodInfo> methDict;
        public Dictionary<string, string, MethodInfo> methDict_sig;
        //public Dictionary<string, MethodBuilder> methDict = new Dictionary<string, MethodBuilder>();
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
            if (memberDict.ContainsKey(name)) throw new InvalidSyntaxException(CS0102, list.Index, _typeName, name);
            memberDict.Add(name, memType, member);
        }
		
		void registerBasicDataTypes(){
			/// Fill the dictionary
			// Also fill in Object, Null, Collections (with full path)
		}
		
		public static void readFile(string path){
			// ....
		}

        #endregion
		#region Scope
        // Whyever it works, it works with push using ExeBuilder.kw and pop using Parser.kw

        /// <summary>This is the current scope.</summary>
        /// <remarks>A field because the Stack shouldn't be manipulated from outside, and ldfld costs 5, whereas .Peek() costs ldLoc, call Peek, stlocXY</remarks>
        public Scope _scope;
        // Just to take notes of the braces.
        Stack<Scope> _scopeStack = new Stack<Scope>();
        //                case (Scope.\w+):\r\n                    break;\r\n
        //            {\1, new List<Scope>() { \1,  } },\r\n
        /// <summary>This dictionary contains the allowed Scope transitions.</summary>
        static Dictionary<Scope,List<Scope>> ScopeStates = new Dictionary<Scope,List<Scope>>() {
            {Scope.Assembly, new List<Scope> { Scope.Namespace, Scope.Type} },
            {Scope.Namespace, new List<Scope> { Scope.Namespace, Scope.Type } },
            {Scope.NoImports, new List<Scope> { Scope.Namespace, Scope.Type } },
			
            {Scope.Member, new List<Scope> { Scope.Type, Scope.Method } }, // Field, Prop, Event
			
            {Scope.Type, new List<Scope> { Scope.Class, Scope.AbstractClass,
                Scope.StaticClass, Scope.Interface, Scope.Enum, Scope.Struct  } },
            {Scope.Class, new List<Scope>       { Scope.Type, Scope.Method, Scope.Member } },
            {Scope.AbstractClass, new List<Scope> { Scope.Type, Scope.Method } }, // Really required?
            {Scope.Struct, new List<Scope>      { Scope.Method } },
            {Scope.StaticClass, new List<Scope> { Scope.Type, Scope.Method } }, // Really required?
            {Scope.Interface, new List<Scope>   { Scope.Method } }, // Interfaces cannot declare types
            {Scope.Enum, new List<Scope> {  } },
			
            {Scope.Method, new List<Scope> { Scope.MethodBody, Scope.ConstructorBody, Scope.AbstractMethod } },
            {Scope.MethodBody, new List<Scope> { Scope.Nested } }, // This scope is required to return the correct type and filling in out-Parameters....
            {Scope.ConstructorBody, new List<Scope> { Scope.Nested } },
            {Scope.AbstractMethod, new List<Scope> {  } }, // Just to help ErrorMgmt.
            {Scope.Nested, new List<Scope> { Scope.Nested } }, // ... which these can do for it, but don't have to.
        };
		/*
		Member <<TempScope, used for managing FaultConditions>>
			Type <<A nested Type -or- TypeDecl in Namespace>
				Class
				Struct
				Interface
				Enum
				Delegate
			Method
				MethodBody
				ConstructorBody <<Disallows return Values>>
				AbstractMethod <<SpecialCase, no new scope>>
			Field
			Property <<Allows get and set; includes Indexer?>>
				MethodBody
			??? Event <<Allows add / remove>>
				MethodBody
		//*/
        /// <summary>
        /// Push a new scope onto the stack.<para/>
        /// Note: Member, Type, and Method are temporary states, and only exist for proper exception scoping. They do not need to be popped.
        /// </summary>
        /// <param name="newScope"></param>
        public void pushScope(Scope newScope)
        {
            if (_scopeStack.Count == 0) {
                switch (newScope) {
                    case Scope.Assembly:
                    case Scope.Namespace:
                    case Scope.Type:
                        _scopeStack.Push(newScope);
                        _scope = newScope;
                        return;
                    default:
                        throw new InvalidOperationException("A Namespace can only contain other Namespaces or Types.");
                }
            }
            List<Scope> scopeList;
            Scope exec = newScope; 
            try {
                scopeList = ScopeStates[newScope]; // Check if exists for Dictionary
                scopeList = ScopeStates[exec = _scope]; // Check if exists in StateDict
            } catch(KeyNotFoundException) { throw new NotImplementedException($"Scope {exec} is not yet implemented."); }
            if (!scopeList.Contains(newScope)) throw new InvalidOperationException($"Transfer from {_scope} to {newScope} is not allowed.");

            /**/
            switch (_scope) {
                case Scope.Type:
                case Scope.Method:
                case Scope.Member:
                    _scopeStack.Pop();
                    //_scopeStack.Push(_scope = newScope); return;
                    break;// Since this is temporary, skip the ContextSwitch
                case Scope.Namespace:
                    _scopeStack.Pop();
                    _scopeStack.Push(Scope.NoImports);
                    break;
                default: break;
            }
            _scopeStack.Push(_scope = newScope);
            //if (_scope == Scope.Type | _scope == Scope.Method) return;
            //_scope = s; //*/

            switch (newScope) {
                case Scope.Type: case Scope.Method:
                case Scope.Member: return;

                case Scope.MethodBody: case Scope.ConstructorBody:
                    ilg = method?.GetILGenerator();
                    args = new Dictionary<string, ParameterHelper>();
                    locals = new Dictionary<string, LocalBuilder>();
                    break;
                case Scope.AbstractMethod: newScope = Scope.Class; break;
            }
            ExeBuilder.kw = Parser.kw = ParserStorage.getDict(newScope);
        }

        public Scope peekScope() => _scopeStack.Peek();
        /// <summary>
        /// A <see cref="Stack{T}.Peek"/> that returns the current non-temporary scope
        /// </summary>
        /// <param name="throwIfNotDecl">If true, checks if <see cref="Stack{T}.Peek"/> is a DeclarationScope; if it is not, a <see cref="InvalidOperationException"/> is thrown.</param>
        /// <returns></returns>
        public Scope peekNonTemp() // Split up into peek / peekNonTemp bzw. rename
        {
            Scope ret = _scope;
            var allowed = new List<Scope>() {Scope.Type, Scope.Method, Scope.Member };
            if (allowed.Contains(_scope)) {/*/
                int temp = -1;
                Scope ts = _scope;
                popScope(ref temp);
                ret = _scope;
                pushScope(ts);/*/
                _scopeStack.Pop();
                ret = _scopeStack.Peek();
                _scopeStack.Push(_scope);//*/
            }
            return ret;
        }

        /// <summary>
        /// Pops TopOfStack of the internal Scope; Also seals Methods (Emit OpCodes.Ret) and Types (.CreateType())
        /// respectively, and removes their references from the quickAccess fields.
        /// </summary>
        /// <param name="pos"></param>
        public void popScope() // Maybe restrict access to this?
        {
            int pos = list.Index;
            switch (_scope) {
                case Scope.Assembly: throw new InvalidOperationException("Can't pop Scope below 0");
                case Scope.Namespace: case Scope.NoImports: break;
                case Scope.Type: /*/break;/*/throw new InvalidProgramException("This should not happen: pop(Scope.Type)");
                case Scope.Class:
                    if(tb!= null) tb.CreateType();
                    if (list[pos].Equals(";")) pos++;
                    if (nested.Count != 0) {
                        tb = nested.Pop();
                        if (tb != null) _typeName = tb.Name;
                        else invalidTypeName();
                    } else {
                        tb = null;
                        _typeName = "<none>";
                    }
                    break;
                //case Scope.AbstractClass: 
                //case Scope.Interface:
                //case Scope.Enum: goto default;
                case Scope.Method: /*/break;/*/throw new InvalidProgramException("This should not happen: pop(Scope.Method)");
                case Scope.AbstractMethod: break;
                case Scope.MethodBody: // This includes Accessors in Properties.
                case Scope.ConstructorBody:
                    //if (ilg != null) {
                        ilg?.Emit(OpCodes.Ret);
                        ilg = null;
                    // }
                    method = null;
                    if (list[pos].Equals(";")) ISE(CS1597, pos);
                    break;
                //case Scope.Nested:
                case Scope.Member:
                    //_scopeStack.Pop();
                    return; // Return, as this should be a tempScope anyway
                default: throw new NotImplementedException($"Add {_scope} to popScope()");
            }
            _scopeStack.Pop();
            _scope = _scopeStack.Peek();

            Parser.kw  = ExeBuilder.kw = ParserStorage.getDict(_scope);
        }
        public void forcePop([System.Runtime.CompilerServices.CallerMemberName] string s = "")//, [System.Runtime.CompilerServices.CallerLineNumber] int i = 0)
        {
            if (s == "execute") _scopeStack.Pop();
            else throw new MethodAccessException("Not allowed to use this method.");
        }
        #endregion

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
            if (name.Equals("this")) {
                string[] formats, args; /**/
                if (method.IsStatic) {
                    formats = new string[4];
                    args = new string[4];
                } else {
                    formats = new string[3];
                    args = new string[3];
                }/*/
                if (method.IsStatic) args = formats = new string[4];
                else args = formats = new string[3];//*/
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
            }
            bool isFree = locals.ContainsKey(name) || args.ContainsKey(name);

            return true;
        }

        /// <summary>Get a list of all currently declared methods in the still unsealed <see cref="TypeBuilder"/>. (includes .ctor and .cctor). Note: Whysoever it is not possible to find anything where other members are saved.</summary>
        /// <param name="tb">TypeBuilder to get methods from.</param>
        /// <returns>A list containing <see cref="MethodBuilder"/>s of all defined methods, including .ctor and .cctor</returns>
        public List<MethodBuilder> GetMethods(TypeBuilder tb)
        {
            var f = (typeof(TypeBuilder).GetField("m_listMethods", BindingFlags.NonPublic | BindingFlags.Instance));
            return (f.GetValue(tb)) as List<MethodBuilder>;
        }

        public override string ToString() => ToString(false);
        /// <summary>ToString method.</summary>
        /// <param name="rollMembers">Whether or not to print Memberinformation.</param>
        /// <returns></returns>
        public string ToString(bool rollMembers)
        {
            var sb = new System.Text.StringBuilder("usings: ");
            if (rollMembers) {
                sb.AppendLine();
                foreach (var item in usings) {
                    sb.AppendLine($" - {item}.*");
                }
            } else sb.Append(usings.Count).AppendLine();
            var types = mb.GetTypes();
            foreach (var item in types) {
                sb.Append($"{item.ToString()}");
                sb.Append(Reflection.sneakType(item, "\r\n - "));
                sb.AppendLine(); // Somehow add number of different members
                if (rollMembers) {
                    sb.Append(Reflection.printAllMembers(item));
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="localOnly">Whether or not to include derived members when printing.</param>
        /// <param name="fullDocu">Additionally creates files for XML-Documentation</param>
        public void ToFile(string path, bool localOnly = false, bool fullDocu = false)
        {
            var parts = path.Split('.');
            string path2 = "";
            this.ToString().WriteText(path);
            foreach (var item in mb.GetTypes()) {
                path2 = $"{parts[0]}_{item.Name}.{parts[1]}";
                var ba = (localOnly) ? Reflection.DeclaredOnlyLookup : Reflection.LookupAll;
                if (fullDocu) Reflection.printDocumentation(item, ba, true, path2);
                else Reflection.printMembers(item, path2, ba);
            }
        }
    }
	
	public abstract class LanguageOptions {
		
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
		public RWDictionary<enumMetaRef, string> metaRef = new RWDictionary<enumMetaRef, string>() {
			// This
			// Base
		};
		
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
		public string makeArray(MetaType mt);
		
		/// <summary>Contains the Type representing the Root object type </summary>
		public string Reference => metaDict[enumMetaTypes.Reference];
		
		/// <summary>Contains the Type representing Compiletime Dynamics</summary>
		public string Dynamic_Compile => metaDict[enumMetaTypes.Dynamic_Compile];
		/// <summary>Contains the Type representing Runtime Dynamics (Duck Typing)</summary>
		public string Dynamic_Runtime => metaDict[enumMetaTypes.Dynamic_Runtime];
		
		
		#endregion
		
		#region DemoStuff
		private string fillDict_demo(){
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
		}
		
		
		// References & Literals
		/// <summary>Contains the Type for an empty Reference</summary>
		public string Null => "null";//metaDict[enumMetaTypes.Null];
		/// <summary>Contains the Type representing boolean Truth</summary>
		public string True => "true";//metaDict[enumMetaTypes.True];
		/// <summary>Contains the Type representing boolean Untruth</summary>
		public string False => "false";//metaDict[enumMetaTypes.False];
		public string This => "this";
		public string Base(string args) => "base("+args+")";
		public string Super(string args) => Base(args);
		public string Default(string args) => "default("+args+")";
		public string NameOf(string args) => "nameof("+args+")";
		public string TypeOf(string args) => "typeof("+args+")";
		public string SizeOf(string args) => "sizeof("+args+")";
		
		#endregion
		
		string _langName = "";
		string _suffix = "";
		public string LangName => _langName;
		public string Suffix => _suffix;
		
		public abstract LanguageOptions(string name, string suffix);
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