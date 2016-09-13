using System;
using System.Collections.Generic;
using System.IO;
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
        public static bool catchedErrors = false;
        

        public static void printer(string name, string output)
        {
            try { System.IO.File.WriteAllText($"myLog_{DateTime.Now.ToFileTime()}_{name}.txt", output); } catch (Exception) { }
        }
        enum Languages
        {
            CSharp, Java, VBNet
        }

        private static void DirectoryCopy(string srcDir, string destDir, bool copySubDirs = false)
        {
            DirectoryInfo dir = new DirectoryInfo(srcDir);
            DirectoryInfo[] dirs = dir.GetDirectories();
            
            if (!dir.Exists) {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + srcDir);
            }
            
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
            
            foreach (FileInfo file in dir.GetFiles()) {
                string temppath = Path.Combine(destDir, file.Name);
                file.CopyTo(temppath, true);
            }

            if (copySubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    string temppath = Path.Combine(destDir, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        static void Main(string[] args)
        {
            string code, codeFile;
            code = (args.Length != 0) ? args[0] : Languages.CSharp.ToString().ToLower();

            codeFile = (args.Length > 1) ? args[1] : "codefile.txt";

            Logger.resetLog();
            if (args.Length == 0) {
                DirectoryCopy("../../MetaCode","./MetaCode");
                File.Delete("MetaCode/#Explain.cs");
                File.Delete("MetaCode/CodeBase_base.cs");
                File.Delete("MetaCode/CodeBase_Reader.cs");
                File.Delete("MetaCode/ICode.cs");
                File.Delete("MetaCode/MetaCode.7z");
                File.Delete("MetaCode/TextFile1.txt");
                new MetaLanguageParser.Parser().execute(codeFile, code);
            } else {

                try {
                    new MetaLanguageParser.Parser().execute(codeFile, code);
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
            if (catchedErrors) {
                Console.WriteLine($"Some non-fatal Errors appeared during Translation. Look into {Logger.path} for details.");
            }
            Console.WriteLine("Finished. Press the AnyKey-Key to close this window.");
            Console.Read();
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

        public static void writeToFile(string path, string contents)
        {
            try { File.WriteAllText(path, contents); File.Delete("WriteError.txt"); } catch (Exception e) {
                Logger.logData(new StringBuilder("Could not write to OutputFile!").AppendLine().Append(e.Message).ToString());
                File.WriteAllText("WriteError.txt", contents);
            }
        }

        public static void LogNonFatalException<T>(T ex) where T : Exception
        {
            Console.Out.Write(ex.Message);
            Logger.logException(ex);
            Program.catchedErrors = true;
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
