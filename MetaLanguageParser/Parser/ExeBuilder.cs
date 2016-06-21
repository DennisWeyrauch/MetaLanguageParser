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

namespace MetaLanguageParser
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



        /// <summary> string: nameToken as found ## Type: Type object representing the token.</summary>
        public Dictionary<string,Type> resolvedTypes;
        List<string> usings;
        
        string _typeName;
        public void invalidTypeName() { _typeName = "<invalid-global-code>"; }
        
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
        public ExeBuilder(ListWalker lw, string langCode)//, IndentedTextWriter write)
        {
            list = lw;
            LangCode = langCode;
        }

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
        internal Stack<CodeBase> codeBase = new Stack<CodeBase>();

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
            } catch (KeyNotFoundException) { throw new NotImplementedException($"Scope {exec} is not yet implemented."); }
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
                case Scope.Type:
                case Scope.Method:
                case Scope.Member: return;

                case Scope.MethodBody:
                case Scope.ConstructorBody:
                    args = new Dictionary<string, MetaType>();
                    locals = new Dictionary<string, MetaType>();
                    break;
                case Scope.AbstractMethod: newScope = Scope.Class; break;
            }
            //ExeBuilder.kw = Parser.kw = ParserStorage.getDict(newScope);
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
                    //if (tb != null) tb.CreateType();
                    if (list[pos].Equals(";")) pos++;
                    /*if (nested.Count != 0) {
                        tb = nested.Pop();
                        if (tb != null) _typeName = tb.Name;
                        else invalidTypeName();
                    } else {
                        //tb = null;
                        _typeName = "<none>";
                    }//*/
                    break;
                //case Scope.AbstractClass: 
                //case Scope.Interface:
                //case Scope.Enum: goto default;
                case Scope.Method: /*/break;/*/throw new InvalidProgramException("This should not happen: pop(Scope.Method)");
                case Scope.AbstractMethod: break;
                case Scope.MethodBody: // This includes Accessors in Properties.
                case Scope.ConstructorBody:
                    //method = null;
                    //if (list[pos].Equals(";")) ISE(CS1597, pos);
                    break;
                //case Scope.Nested:
                case Scope.Member:
                    //_scopeStack.Pop();
                    return; // Return, as this should be a tempScope anyway
                default: throw new NotImplementedException($"Add {_scope} to popScope()");
            }
            _scopeStack.Pop();
            _scope = _scopeStack.Peek();

            //Parser.kw = ExeBuilder.kw = ParserStorage.getDict(_scope);
        }
        public void forcePop([System.Runtime.CompilerServices.CallerMemberName] string s = "")//, [System.Runtime.CompilerServices.CallerLineNumber] int i = 0)
        {
            if (s == "execute") _scopeStack.Pop();
            else throw new MethodAccessException("Not allowed to use this method.");
        }
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

        #region FileProperties

        
        #endregion
    }
}