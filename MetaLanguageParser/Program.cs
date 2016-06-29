using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLanguageParser
{
    class Program { 
            
        /// <summary>
        /// Printer Flag to output the generator steps
        /// </summary>
        public static bool printParts = false;

        public static void printer(string name, string output)
        {
            try { System.IO.File.WriteAllText($"myLog_{DateTime.Now.ToFileTime()}_{name}.txt", output); } catch (Exception) { }
        }

        static void Main(string[] args)
        {
            string code;
            code = (args.Length != 0) ? args[0] : "vbnet";
            Logger.resetLog();
            if (args.Length == 0) {
                new MetaLanguageParser.Parser().execute("codefile.txt", code);
            } else {

                try {
                    new MetaLanguageParser.Parser().execute("codefile.txt", code);
                } catch (KeyNotFoundException knfe) {

                    Logger.logKeyException(knfe);
                    Console.Read();
                    //throw;
                }/**/
                catch (Exception e) {
                    Console.Out.Write(e.Message);
                    Logger.logException(e);
                    if (args?.Length == 0) throw;
                    else Console.WriteLine($"Build was not successful. Look into {Logger.path} for details.");
                    Console.Read();
                }//*/
            }
        }
    }
    // Comments are in snippet File //
    public static class Logger
    {
        public static string path { get; set; } = "logger.log";
        public static void resetLog() => System.IO.File.Delete(path);
        public static void logData(this string str)
                => System.IO.File.AppendAllText(path, str);
        public static void logException<T>(T ex) where T : Exception
        {
            var sb = new StringBuilder().AppendLine().AppendLine();
            sb.Append("# ExceptionType: ").AppendLine(ex.GetType().Name);
            sb.Append("# Message: ").AppendLine(ex.Message);
            sb.Append("# Source: ").AppendLine(ex.Source);
            sb.Append("# TargetSite: ").AppendLine(ex.TargetSite.Name);
            sb.Append("# StackTrace: ").AppendLine(ex.StackTrace);
            if (ex.Data.Count > 0) {
                sb.AppendLine("## Data: ");
                foreach (var item in ex.Data) {
                    sb.AppendLine(item.ToString());
                }
            }
            logData(sb.ToString());
        }
        public static void logKeyException<T>(T ex) where T : KeyNotFoundException
        {
            var sb = new StringBuilder().AppendLine().AppendLine();
            var stacktrace = ex.StackTrace;
            var mypos = stacktrace.IndexOf("MyResourceReader.get___")+"MyResourceReader.get___".Length;
            var endPos = stacktrace.IndexOf("()", mypos);
            var key = stacktrace.Substring(mypos, endPos-mypos).ToLower().ToCharArray();
            // Specific KeyTransformator:
            key[0] = char.ToUpper(key[0]);
            var message = ex.Message + $": {string.Concat(key)}";
             Console.Out.WriteLine(message);
            sb.Append("# Message: ").AppendLine(message);
            sb.Append("# StackTrace: ").AppendLine(ex.StackTrace);
            if (ex.Data.Count > 0) {
                sb.AppendLine("## Data: ");
                foreach (var item in ex.Data) {
                    sb.AppendLine(item.ToString());
                }
            }
            logData(sb.ToString());
        }
    }
}
