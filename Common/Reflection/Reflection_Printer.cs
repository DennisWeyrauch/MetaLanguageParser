using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System.IO;
using System.Text.RegularExpressions;

namespace Common.Reflection
{
    public partial class Reflection
    {
        #region Print         (4961/4987 in 85)
        /// <summary>see <see cref="printMembers(Type, string, BindingFlags)"/> with 
        /// (<paramref name="t"/>, <paramref name="path"/>, <see cref="LookupAll"/>)</summary>
        public static void printAllMembers(Type t, string path) => printMembers(t, path, LookupAll);
        /// <summary>see <see cref="printMembers(Type, string, BindingFlags)"/> with 
        /// (<paramref name="t"/>, <paramref name="path"/>, <see cref="LookupAll"/>)</summary>
        public static string printAllMembers(Type t) => printMemberDict(Reflection.GetMembers(t, LookupAll));
        /// <summary>see <see cref="printMembers(Type, string, BindingFlags)"/> with 
        /// (<paramref name="t"/>, <paramref name="path"/>, <see cref="DeclaredOnlyLookup"/>)</summary>
        public static void printLocalMembers(Type t, string path) => printMembers(t, path, DeclaredOnlyLookup);
        /// <summary>
        /// Retrieves all members of type <paramref name="t"/> and prints them grouped per <see cref="MemberTypes"/>.
        /// </summary>
        /// <param name="t">The type to scan</param>
        /// <param name="path">Filename (or path) to write output into.</param>
        /// <param name="bindingAttr">Level of visibility to collect mebers of</param>
        public static void printMembers(Type t, string path, BindingFlags bindingAttr)
        {
            printMemberDict(Reflection.GetMembers(t, bindingAttr), path);
        }
        /// <summary>
        /// Format and print the contents of <paramref name="dict"/> into a given file
        /// </summary>
        /// <param name="dict">Dictionary to read from</param>
        /// <param name="path">File to write into</param>
        public static void printMemberDict(Dictionary<MemberTypes, MemberInfo[]> dict, string path)
            => printMemberDict(dict).WriteText(path);
        /// <summary>
        /// Format and print the contents of <paramref name="dict"/> into a given file
        /// </summary>
        /// <param name="dict">Dictionary to read from</param>
        /// <param name="path">File to write into</param>
        public static string printMemberDict(Dictionary<MemberTypes, MemberInfo[]> dict)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict), "No dictionary given");
            var sb = new System.Text.StringBuilder();
            foreach (var dictElem in dict) {
                sb.AppendLine($"## {dictElem.Key.ToString()} - {dictElem.Value.ToString()}");
                foreach (var memArray in dictElem.Value) {
                    sb.AppendLine(memArray.ToString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Uses the internal documentation feature to extract the XML-documentation
        /// </summary>
        /// <param name="path">Path to file to use for parsing</param>
        /// <param name="dest">File to (create and) write Results into. Default is filename of "<paramref name="path"/>" + '.xml' </param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null</exception>
        /// <exception cref="FileNotFoundException">Either "<paramref name="path"/>" was not found, or does not contain valid code</exception>
        public static void genDocu(string path, string dest = "")
        {
            if (path.IsNOE()) throw new ArgumentNullException(nameof(path), "No file to parse was given.");

            System.CodeDom.Compiler.CompilerResults res = null;
            //var name = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var name = path.Substring(path.LastIndexOf('\\')+1);
            try {
                //http://stackoverflow.com/questions/32505209/how-can-i-use-latest-features-of-c-sharp-v6-in-t4-templates
                using (CSharpCodeProvider foo = new CSharpCodeProvider()) {//options)) {
                    var comPar = new System.CodeDom.Compiler.CompilerParameters();
                    var dictAss = Assembly.GetAssembly(typeof(RWDictionary<object, object>));
                    var curAss = Assembly.GetExecutingAssembly();
                    var sb = new System.Text.StringBuilder();
                    if (dest.IsNotNOE()) {
                        sb.Append($"/doc:{dest}");
                        if (!dest.EndsWith(".xml")) sb.Append(".xml");
                    } else sb.Append($"/doc:{name}.xml");
                    //sb.Append($" /target:{name}");
                    comPar.CompilerOptions = sb.ToString();

                    comPar.ReferencedAssemblies.Add("System.dll");
                    comPar.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                    comPar.ReferencedAssemblies.Add("mscorlib.dll");
                    comPar.ReferencedAssemblies.Add("System.Core.dll");
                    comPar.ReferencedAssemblies.Add(curAss.Location);
                    comPar.ReferencedAssemblies.Add(dictAss.Location);

                    res = foo.CompileAssemblyFromFile(comPar, path);
                }
            } catch (FileNotFoundException) {
                var str = $"{name}.log";
                if (File.Exists(str)) File.Delete(str);
                var sb = new System.Text.StringBuilder( );
                foreach (var item in res.Output) {
                    sb.AppendLine(item);
                }
                (sb.ToString()).AppendLine(str);
            }
        }

        /// <summary>
        /// Prints an XML-File with all public members of the given type, listing all type and member attributes.
        /// </summary>
        /// <param name="t">Type to parse</param>
        /// <param name="methodsFirst">Whether or not the methods should be printed first or not. Default true</param>
        public static void printDocumentation(Type t, bool methodsFirst = true)
        {
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            printDocumentation(t, bindingAttr, methodsFirst);
        }

        static MethodInfo __ForNAndSig;
        /// <summary>
        /// Prints an XML-File with all members visible at scope <paramref name="bAttr"/> of the given type, listing all type and member attributes.
        /// </summary>
        /// <param name="t">Type to parse</param>
        /// <param name="bAttr">Scope to search at</param>
        /// <param name="methodsFirst">Whether or not the methods should be printed first or not. Default true</param>
        public static void printDocumentation(Type t, BindingFlags bAttr, bool methodsFirst = true, string path = "")
        {
            if (t == null) throw new ArgumentNullException($"{t} is null");
            var dict = Reflection.GetMembers(t, bAttr, methodsFirst);
            var sb = new StringBuilder();
            char defPrefix = 'X';
            string i = "    "; // Indent
            string i2 = $"\r\n{i}{i}", i3 = $"\r\n{i}{i}{i}";
            //string sumH = $"{i3}<summary>", sumT = "</summary>\r\n"; //"";// 
            string attr = "", retH = $"{i3}<returns type=\"", retT = "\"/>";// "\"></returns>\r\n"; //

            /** The following will happen with special cases:
			 * Accessors of properties will be turned into "get_$$Name$()" and, if added, "set_$$NAME$($$BaseType$)"
			 * Likewise, Indexers will be turned into methods as well, as "get_Item($$IdxType$)" und "set_Item($$IdxType$, $$BaseType$)"
			 * "ref"-Keyword adds "ByRef"
			 * Constructors are ".ctor", Destructors ".dtor" and the hidden TypeConstructor would be ".cctor"
			 * The generic <> are turned into: 
			 ** Filled in: List<object> --> `1[System.Object]
			 ** Open generic: $$Method$<T> --> $$Method$[T]
			**/
            sb.Append($"<?xml version=\"1.0\"?>\r\n<doc>\r\n{i}<assembly><name>{t.FullName}</name></assembly>\r\n{i}<members>");
            sb.Append($"{i2}<member name=\"T:{t.FullName}\">");
            sb.Append($"{sneakType(t, i3)}{i2}");
            sb.Append("</member>\r\n");
            if (__ForNAndSig == null) {
                __ForNAndSig = new Reflection().FindMethod(typeof(MethodBase), "FormatNameAndSig", Type.EmptyTypes);
            }
            MethodInfo FormatNameAndSig = __ForNAndSig;
            foreach (var dictElem in dict) { // Iterating over MemberCategories
                foreach (var memArray in dictElem.Value) { // Iterating over Member-Array
                    attr = __getAttributes(memArray, i);
                    sb.Append($"{i}{i}<member name=\"");
                    switch (memArray.MemberType) {
                        case MemberTypes.NestedType: // ((System.Reflection.MethodBase)memArray).FullName
                            var n = (TypeInfo)memArray;
                            sb.Append($"N:{n.FullName}\">{sneakType(t, i3)}");
                            printDocumentation(n.UnderlyingSystemType);
                            break;
                        case MemberTypes.Constructor: // <$TYPE$>#.ctor() // <$TYPE$>#.ctor(PARAMS)
                            var c = (MethodBase)memArray;
                            //var cc = new Reflection().invokeMethod<string, MethodBase>(c, "FormatNameAndSig");
                            var cc = (string) FormatNameAndSig.Invoke(c, Type.EmptyTypes);
                            sb.Append($"C:{cc}\">{attr}");
                            break;
                        case MemberTypes.Method:
                            var m = (MethodBase)memArray;
                            //var xx = new Reflection().invokeMethod<string, MethodBase>(m, "get_FullName");
                            //var xx = new Reflection().invokeMethod<string, MethodBase>(m, "FormatNameAndSig");
                            var xx = (string) FormatNameAndSig.Invoke(m, Type.EmptyTypes);
                            var ret = (m as MethodInfo).ReturnType;
                            sb.Append($"M:{xx}\">{attr}{retH}");
                            if (ret.ContainsGenericParameters) {
                                if (ret.HasElementType) {
                                    var r = ret.GetElementType();
                                    sb.Append(r.Name + printGeneric(r));
                                    if (ret.IsArray) sb.Append("[]");
                                    if (ret.IsPointer) sb.Append("*");
                                } else sb.Append(ret.Name + printGeneric(ret));
                            } else sb.Append(ret.Name);
                            sb.Append(retT);
                            break;
                        case MemberTypes.Property: // Unknown if neccessary, since accessors are methods and thus listed at top
                            sb.Append($"P:{t.FullName}.{memArray.Name}\">{attr}");
                            //sb.Append($"{sum}{retH}{m.ReturnType.Name}{retT}");
                            break;
                        case MemberTypes.Event: defPrefix = 'E'; goto default;
                        case MemberTypes.Field: defPrefix = 'F'; goto default;
                        default: sb.Append($"{defPrefix}:{memArray.ToString()}\">{attr}"); break;
                    } // Member-Switch
                    sb.Append($"{i2}</member>\r\n");
                } // Member-Loop
            } // Dictionary-Loop
            sb.Append($"{i}</members>\r\n</doc>");
            string savepath = $"doc\\{t.FullName}.xml";
            if (path.IsNotNOE()) {
                savepath = path + ".xml";
            }
            sb.ToString().WriteText(savepath);
        }

        static string printGeneric(Type t)
        {
            if (t.IsGenericParameter) return "";
            var sb = new StringBuilder("[");
            string comma = "";

            Type[] ta = null;
            if (t.IsGenericTypeDefinition) {
                dynamic x = t; // Open generic types are RunTimeTypes
                ta = x.GenericTypeParameters;
            } else if (t.IsGenericType) { ta = t.GenericTypeArguments; }
            foreach (var item in ta) {
                sb.Append($"{comma}{item.Name}");
                if (item.IsGenericType) sb.Append(printGeneric(item));
                comma = ", ";
            }
            sb.Append("]");
            return sb.ToString();
        }

        //string iLv(int lv, string indent = "    ") => new StringBuilder()
        public static void trimCode(string input)
        {
            StringBuilder result = new StringBuilder();
            //string indent = "    ";
            //int indLv = 0;
            // ReadIn File
            // Remove any comments
            string s1 = Regex.Replace(input, @"(.*?)(?:(///.*?((\s*//.*?)*)?)\r?\n|(//.*?((\s*//.*?)*)?)\r?\n|(/[*]+[\s\S]*?[*]+/))", @"$1$2\r\n");
            // Remove Trailing Whitespace
            string s2 = Regex.Replace(s1, @"[ \t]*\r\n", @"\r\n");
            string s2b = Regex.Replace(s2, @"\t", @"    ");
            // Remove multiple Linebreaks
            string s3 = Regex.Replace(s2b, @"(\r\n){3,}", @"\r\n\r\n");

            // Remove MethodBody
            string s4 = Regex.Replace(s3, @"((public|protected) (?!(partial )?(class|interface))[\s\S]*?)\s*(=> [^;]+;|([{]([^{}]*((?4)|[^{}]*))*[}]))", @"$1");

            //(result.ToString()).WriteText("trimmed.cs");
            s4.WriteText("trimmed.cs");
        }

        #endregion
    }
}
