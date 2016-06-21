using System; // Exception, Console
using System.IO; // FileNotFoundException, File
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform; // CSharpCodeProvider
using System.Reflection; // Assembly
// CompilerResults, CompilerParameter


namespace Common.Reflection
{
    public partial class Reflection
    {
        #region 1-executeCCtor  (5117/5136 in 90)
        /// <summary>
        /// Execute an embeddded constructor
        /// Possible Exceptions: FileNotFoundException (Invalid Code), 
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="code"/> is invalid or the specified entry point could not be found</exception>
        /// <param name="code">The code to execute</param>
        /// <param name="startType">The type that contains the constructor</param>
        /// <param name="param">Array of method parameters</param>
        public void executeCCtor(string codeRaw, string startType, object[] param) => getInstance(codeRaw, startType, param);

        /// <summary>
        /// Generates a factory method for and executes it to retrieve an instance of <paramref name="startType"/>.
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="code"/> is invalid or the specified entry point could not be found</exception>
        /// <param name="codeRaw">The code to execute</param>
        /// <param name="startType">The type that contains the constructor</param>
        /// <param name="param">Array of method parameters</param>
        public object getInstance(string codeRaw, string startType, object[] param)
        {
            var sb = new System.Text.StringBuilder();
            if (param?.Length > 0) {
                //sb.Append(param[0]);
                string comma = "";
                for (int i = 0; i < param.Length; i++) {
                    if (param[i].GetType().FullName.Equals("System.String")) {
                        var x = (param[i] as string).Split('\\');
                        var xx = string.Join("\\\\", x);
                        sb.Append($"{comma}\"{xx}\"");
                    } else sb.Append(comma + param[i]);
                    comma = ", ";
                }
            }
            if (Namespace != null) {
                if (!Namespace.EndsWith(".")) Namespace += ".";
            } else Namespace = "";
            //codeRaw = codeRaw + $" public class FooClass {{ public void Execute() {{ new CSharpTokenizer.{startType}(\"{startType}\", {sb.ToString()}).buildText();}} }}";

            codeRaw = codeRaw + $" public class FooClass {{ public static object Execute() {{ return new {Namespace}{startType}({sb.ToString()});}} }}";
            var type = compileCode(codeRaw).GetType("FooClass");
            return type.GetMethod("Execute").Invoke(null, new object[] { });

            //var type = compileCode(codeRaw).GetType(startType);
            //var obj = Activator.CreateInstance(type);
            //var output = type.GetMethod("Execute").Invoke(obj, new object[] { });

            //return type.GetConstructor(Type.EmptyTypes).Invoke(param); // Not feasable, because it TypeArray, and Namespace and whatnot.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codeRaw"></param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <returns></returns>
        internal static Assembly compileCode(string codeRaw)
        {
            string code = escapeCode(codeRaw);
            System.CodeDom.Compiler.CompilerResults res = null;
            try {
                //http://stackoverflow.com/questions/32505209/how-can-i-use-latest-features-of-c-sharp-v6-in-t4-templates
                //var options = new Dictionary<string, string>{{"CompilerVersion", "v4.6"}};
                using (CSharpCodeProvider foo = new CSharpCodeProvider()) {//options)) {
                    var comPar = new System.CodeDom.Compiler.CompilerParameters();
                    var dictAss = Assembly.GetAssembly(typeof(RWDictionary<object, object>));
                    var curAss = Assembly.GetExecutingAssembly();

                    comPar.CompilerOptions = "/optimize";
                    comPar.GenerateInMemory = true;
                    comPar.ReferencedAssemblies.Add("System.dll");
                    comPar.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                    comPar.ReferencedAssemblies.Add("mscorlib.dll");
                    comPar.ReferencedAssemblies.Add("System.Core.dll");
                    comPar.ReferencedAssemblies.Add(curAss.Location);
                    comPar.ReferencedAssemblies.Add(dictAss.Location);

                    res = foo.CompileAssemblyFromSource(comPar, code);
                    return res.CompiledAssembly;
                }
            } catch (FileNotFoundException) {
                var sw = File.CreateText(file_errorLog);
                sw.WriteLine($"The compilation has thrown errors. (foundCond = {foundCond}; foundDEBUG = {foundDebug})\r\n");
                foreach (var item in res.Errors) {
                    Console.WriteLine(item.ToString());
                    sw.WriteLine(item.ToString());
                }
                if (_doDebug) {
                    sw.WriteLine();
                    sw.WriteLine("Note: The code given in error_code.log matches exact the one that threw the errors");
                }
                sw.Close();
            } catch (Exception e) {
                var sw = File.CreateText(file_errorLog);
                var msg = $"Unable to compile Code into .dll;\r\n {e.GetType().Name} :\r\n\t{e.Message}";
                Console.WriteLine(msg);
                sw.WriteLine(msg);
                sw.Close();
            }
            if (_doDebug) code.WriteText(file_errorCode);
            //Console.Read(); throw new ArgumentException("Invalid code");
            return null;
        }

        #endregion
    }
}
