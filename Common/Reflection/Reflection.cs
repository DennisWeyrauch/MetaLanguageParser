using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Reflection
{
    using static Common.Extensions;
    //http://www.drdobbs.com/generating-code-at-run-time-with-reflect/184416570
    public partial class Reflection
    {
        // //Common.Properties.Resources.file_errorLog
        static string file_errorCode = "errorCode.cs";
        static string file_errorLog = "errorLog.log";
        //public Type _returnType = null;
        //public object _returnObj = null;

        static bool _doDebug = false;
        public static bool doDebug => _doDebug; // Just so that you can't invoke the setter
        public static void setDebug(bool state, [System.Runtime.CompilerServices.CallerMemberName] string s = "") {
            //var x = s.Substring(s.LastIndexOf('\\')+1);
            if (s.Equals("makeList")) _doDebug = state;
            else Console.WriteLine($"AccessViolationException ({s}) : Access to Debug-Flag denied.");
        }

        public Reflection() { }

        /// <summary>
        /// Emits (Public) (Instance/Static) (Derived/Declared) members
        /// </summary>
        public const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        /// <summary>
        /// Emits (Public/Protected/Private/Internal) (Instance/Static) (DeclaredOnly) members
        /// </summary>
        public const BindingFlags DeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        /// <summary>
        /// Emits (Public/Protected/Private/Internal) (Static) (Derived/Declared) members
        /// </summary>
        public const BindingFlags LookupStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        /// <summary>
        /// Emits (Public/Protected/Private/Internal) (Instance/Static) (Derived/Declared) members
        /// </summary>
        public const BindingFlags LookupAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private string _namespace = null;
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }
        #region 1-executeCCtor  (5117/5136 in 90)
        #endregion
        #region 2-Invoke Method (7651/7670 in 123)
        #endregion

        #region 3-Reflection    (3039/3060 in 65)  [[Core 1]]

        /// <summary>
        /// Reflects types and groups them over <see cref="MemberTypes"/>.
        /// </summary>
        /// <param name="codeRaw">Code to compile and get Memberinfo from</param>
        /// <exception cref="TargetException"><paramref name="codeRaw"/> does not contain valid code.</exception>
        /// <returns>An <see cref="RWDictionary{TKey, TValue}"/> containing Pairs of "TypeName : MemberDictionary"</returns>
        public static Dictionary<string, MemberInfo[]> reflectTypesToArray(string codeRaw)
        {
            var dict = new Dictionary<string, System.Reflection.MemberInfo[]>();
            var types = getTypes(codeRaw);
            if (types == null || types.Length == 0) throw new TargetException("Could not find any type-definitions.\r\n");
            foreach (var item in types) {
                dict.Add(item.Name, item.GetMembers(DeclaredOnlyLookup));
            }
            return dict;
        }

        /// <summary>
        /// Reflects types and groups them over <see cref="MemberTypes"/>.
        /// </summary>
        /// <param name="codeRaw">Code to compile and get Memberinfo from</param>
        /// <exception cref="TargetException"><paramref name="codeRaw"/> does not contain valid code.</exception>
        /// <returns>An <see cref="RWDictionary{TKey, TValue}"/> containing Pairs of "TypeName : MemberDictionary"</returns>
        public static RWDictionary<string, Dictionary<MemberTypes, MemberInfo[]>> reflectTypesToDict(string codeRaw)
        {
            var dict = new RWDictionary<string, Dictionary<MemberTypes, MemberInfo[]>>();
            var types = getTypes(codeRaw);
            if (types == null || types.Length == 0) throw new TargetException("Could not find any type-definitions.\r\n");
            foreach (var item in types) {
                var x = GetMembers(item, DeclaredOnlyLookup);
                dict.Add(item.Name, x);
            }
            return dict;
        }

        public static Dictionary<MemberTypes, MemberInfo[]> GetMembers(Type type, BindingFlags bindingAttr)
			=> GetMembers(type, bindingAttr, true);
        /// <summary>Collects all members visible with <paramref name="bindingAttr"/> and groups them by <see cref="MemberTypes"/>.</summary>
        /// <param name="type">The type to parse</param>
        /// <param name="bindingAttr">Highest scope to collect members from.</param>
        /// <param name="methodsFirst">Determines order of Elements: true(default) = M,C,F,P,Ev,Nested, false = F,P,C,M,Ev,Nested</param>
        /// <returns></returns>
        public static Dictionary<MemberTypes, MemberInfo[]> GetMembers(Type type, BindingFlags bindingAttr, bool methodsFirst)
        {
            if (type == null) throw new ArgumentNullException();
            var dict = new Dictionary<MemberTypes, MemberInfo[]>();
            try {
                MethodInfo[] m      = type.GetMethods(bindingAttr); // Contain also Indexers and Accessors
                ConstructorInfo[] c = type.GetConstructors(bindingAttr);
                FieldInfo[] f       = type.GetFields(bindingAttr);
                PropertyInfo[] p    = type.GetProperties(bindingAttr);
                EventInfo[] e       = type.GetEvents(bindingAttr);
                Type[] t            = type.GetNestedTypes(bindingAttr);
                if (methodsFirst) {  // M, C, F, P, E, Nested
                    dict.Add(MemberTypes.Method, m);
                    dict.Add(MemberTypes.Constructor, c);
                }
                dict.Add(MemberTypes.Field,       f);
                dict.Add(MemberTypes.Property,    p);
				if(!methodsFirst){ // F, P, C, M, E, Nested
                    dict.Add(MemberTypes.Constructor, c);
                    dict.Add(MemberTypes.Method,      m);
				}
                dict.Add(MemberTypes.Event,       e);
                dict.Add(MemberTypes.NestedType,  t);
            } catch (Exception e) {
                Console.WriteLine($"{e.GetType().Name} : {e.Message}");
            }
            return dict;
        }

        /// <summary>Tries to compile <paramref name="codeRaw"/> and returns the resulting type array.</summary>
        /// <param name="codeRaw">Code to compile</param>
        /// <returns>Array of types found in the resulting assembly.</returns>
        internal static Type[] getTypes(string codeRaw) => compileCode(codeRaw).GetTypes();
        
        #endregion
        #region 4-Escaping Code                    [[Core 2]]
        /// <summary>
        /// Remove LineBreaks, Tabs and LineComments (except if they are in Strings).
        /// Also merges using directives in case of multiple codeFiles. 
        /// BlockComments and surplus Space are ignored anyway by the Compiler. 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static string escapeCode(string code, bool debug = false)
        {
            foundCond = false;
            foundDebug = false;
            if (System.IO.File.Exists("code_with_condComp.cs")) System.IO.File.Delete("code_with_condComp.cs");
            if (doDebug) System.IO.Directory.CreateDirectory("debug");
            //string escStr = "(?(?=@\")(@\"[^\"]*?\"(?:\"[^\"]*?\")*)|(\\$?\"[^\"]*?((?>\\\")[^\"]*|\")))";//@"(""[\s\S]*?(?!\\"")"")";
            //string escStr = "((?>(?(?=@\")(?:@\"[^\"]*?\"(?:\"[^\"]*?\")*)|(?:[$]?\"[^\"]*?(?:(?>\\\")[^\"]*|\")))))";//@"(""[\s\S]*?(?!\\"")"")";
            string escStr = @"(""((?>\\\\)|(?>\\"")|[^""])*?"")";//"(\"([^\\](?>\\\")|[^\"])*?\")";
            if (debug) code.WriteText(@"debug\logA_raw.cs");

            /// Step1: Remove comments (String | BlockComment | [Stacked] LineComments)
            // [^\\] for Escape
            var c1 = Regex.Replace(code, escStr + @"|(?:/[*][\s\S]*?[*]/)|(?://.*?(?:(?:\s*//.*?)*)?)(\r\n)", new MatchEvaluator(evalStr));//"$1");
            if (debug) c1.WriteText(@"debug\logB_noComments.cs");

            /// Step2: Remove Preprocessor Directives (Transform Conditional, remove rest)
            // var c2 = Regex.Replace(c1, @"(""[\s\S]*?"")|^\s*#if [\s\S]*?\r\n\s*(?:#else([\s\S]*?)\r\n|#endif)|^\s*#.*\r\n|\t+", "$1$2", RegexOptions.Multiline); // Not feasable to findout which part should be compiled 
            var c2 = Regex.Replace(c1, @"#if([\s\S]*?)#endif", new MatchEvaluator(Reflection.doEvalCond), RegexOptions.Multiline);
            c2 = Regex.Replace(c2, escStr + @"|^\s*#.*\r\n|\t+", "$1", RegexOptions.Multiline);
            if (debug) c2.WriteText(@"debug\logC_noPraePro.cs");
            if (foundCond) {
                Console.WriteLine("Found Conditional compilation directives. Compilation may fail");
                c2.WriteText(@"code_with_condComp.cs");
            }
            if (foundDebug) {
                Console.WriteLine("Found DEBUG compilation directives. Reduced Code according to debug argument '-d / debug'.");
                c2.WriteText(@"code_with_condComp.cs");
            }

            /// Step3: Organize usings
            var matches = Regex.Matches(c2, @"^(using [^;=]*?;)", RegexOptions.Multiline).Cast<Match>();
            RWDictionary<string,string> dict = new RWDictionary<string,string>();
            string usings = "";
            foreach (var token in from m in matches select m.Groups[1].Value) {
                dict.Add(token, "", true);
            }
            // using alias prop works with Groups[2] etc.

            var c3 = Regex.Replace(c2, escStr + @"|^using .*?;\s*?$", "$1", RegexOptions.Multiline);
            foreach (var item in dict) {
                //if (item.Value.IsNOE()) { // For alias
                usings += item.Key + "\r\n";
                //}
            }
            var c4 = usings + c3;
            if (debug) c4.WriteText(@"debug\logD_usings.cs");

            /// Step5: Remove all Tabs and all Newlines
            if (_doDebug) return Regex.Replace(c4, @"\t+|(@""[^""]*?""(?:""[^""]*?"")*)|\r\n", "$1");
            else return Regex.Replace(c4, @"\t+|(@""[^""]*?""(?:""[^""]*?"")*)|\r\n( )+", "$1$2");
        }
        // Verbatim: (@\"[^\"]*?\"(?:\"[^\"]*?\")*) // (@""[^""]*?""(?:""[^""]*?"")*)
        // String: (?!@")("(\\"|[^"]*)(?<!\\)")

        static string evalStr(Match m)
        {
            if (Regex.IsMatch(m.Value, "^[@$]?\"[\\s\\S]*\"$")) {
                return m.Value;
            }
            return "\r\n";
        }

        static bool foundCond = false;
        static bool foundDebug = false;
        static string doEvalCond(Match match)
        {// ^\s*#if (.*?)\r\n([\s\S]*?)(?=#else|#elif|#endif)|#else ([\s\S]*?)(?=#endif)
            var org = match.Value;
            List<string[]> list = new List<string[]> {
                //new[] {@"(""[\s\S]*?"")", "$1"}, // strings
                new[] {@"#if\s*(.*?)\r\n([\s\S]*?)(?=#elif|#else|#endif)", "if ($1) {\r\n$2" }, // #if
                new[] {@"#elif\s*(.*?)\r\n([\s\S]*?)(?=#elif|#else|#endif)", "} else if ($1) {\r\n$2" }, // #elif
                new[] {@"#else([\s\S]*?)(?=#endif)", "} else {\r\n$1"}, // #else
                new[] {@"#endif", @"}"}  // #endif
            };

            #region At least make Debug work
            int idx = -1; // idx ==> Throw away all except list[idx]
#if false
            if (Regex.IsMatch(org, @"#if\s*true")) idx = 0;
            else if (Regex.IsMatch(org, @"#if\s*false")) idx = 2;
            else if (Regex.IsMatch(org, @"#if\s*[!]?DEBUG")) {
                if (Regex.IsMatch(org, @"#if\s*DEBUG")) idx = (_doDebug) ? 0 : 2;
                if (Regex.IsMatch(org, @"#if\s*!DEBUG")) idx = (_doDebug) ? 2 : 0;
            } else if (Regex.IsMatch(org, @"#elif\s*[!]?DEBUG")) {
                if (Regex.IsMatch(org, @"#elif\s*DEBUG")) idx = (_doDebug) ? 1 : 2;
                if (Regex.IsMatch(org, @"#elif\s*!DEBUG")) idx = (_doDebug) ? 2 : 1;
            } else foundCond = true;
#else
            Dictionary<string, int> dict = new Dictionary<string, int>() {
                { @"#if\s*true", 0 }, { @"#if\s*false", 2 },
                { @"#if\s*DEBUG",   (_doDebug)?0:2}, { @"#if\s*!DEBUG",   (_doDebug)?2:0},
                { @"#elif\s*DEBUG", (_doDebug)?1:2}, { @"#elif\s*!DEBUG", (_doDebug)?2:1},
            };
            foreach (var item in dict) { if (Regex.IsMatch(org, item.Key)) { idx = item.Value; break; } }
#endif
            if (idx != -1) {
                foundDebug = true;
                var rep = (idx == 2)?"$1" : "$2"; // #else has only one ()-pair
                for (int i = 0; i < list.Count; i++) {
                    list[i][1] = (i == idx) ? rep : "";
                }
            } else foundCond = true;
            #endregion
            for (int i = 0; i < list.Count; i++) {
                org = Regex.Replace(org, list[i][0], list[i][1]);
            }
            return org;
        }
        #endregion
        #region 5-Print         (4961/4987 in 85)  [[Print]]
        #endregion
        #region 6-GetAttributes                    [[???]]
        #endregion
        
        /// <summary>
        /// Get the ParameterList of the given Method
        /// </summary>
        /// <param name="mb"></param>
        /// <returns></returns>
        public static ParameterInfo[] getParameters(MethodBase mb) => mb.GetParameters();
        /// <summary>
        /// Get a List of <see cref="LocalVariableInfo"/> of the given <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="mbase"></param>
        /// <returns></returns>
        public static IList<LocalVariableInfo> getLocalVariables(MethodInfo mbase)
        {
            var r = new Reflection();
            MethodBody mb = mbase.GetMethodBody();
            foreach (LocalVariableInfo lvi in mb.LocalVariables) {
                Console.WriteLine("Local variable: {0}", lvi);
            }
            return mb.LocalVariables;
        }
        
        /// <summary>
        /// Set a non-public field called '<paramref name="name"/>' in the given instance of '<paramref name="obj"/>' to '<paramref name="value"/>'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void setPrivateField<T>(T obj, string name, object value)
        {
            var field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(obj, value);
        }
        /// <summary>
        /// Set a non-public static field called '<paramref name="name"/>' of '<paramref name="obj"/>' to '<paramref name="value"/>'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void setPrivateField(Type type, string v, object value)
        {
            var field = type.GetField(v, BindingFlags.Static | BindingFlags.NonPublic);
            // if type.isNotStatic
            field.SetValue(Activator.CreateInstance(type), value);
        }
        /// <summary>
        /// Retrieves a non-public field with the name '<paramref name="name"/>' from <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public static FieldInfo getPrivateField(Type type, string name, bool isStatic)
        {
            return type.GetField(name, (isStatic)?BindingFlags.Static | BindingFlags.NonPublic : BindingFlags.NonPublic);
        }
        

        public static void testReflection()
        {
            var r = new Reflection();
            try {
                var x = Reflection.reflectTypesToDict(System.IO.File.ReadAllText(@"..\Test.cs"));
                var ci = (ConstructorInfo) x[0][MemberTypes.Constructor][0];
                var mi = (MethodInfo) x[0][MemberTypes.Method][0];
                var y = Reflection.getParameters(ci);
                var z = Reflection.getLocalVariables(mi);
                var rmh = new Reflection().invokeMethod<RuntimeMethodHandle, MethodInfo>(mi, "GetMethodHandle");
                var a = new Reflection();
            } catch (Exception e) {
                Console.WriteLine($"{e.GetType().Name} : {e.Message}");
            }
            Console.Read();
        }

    }
}

