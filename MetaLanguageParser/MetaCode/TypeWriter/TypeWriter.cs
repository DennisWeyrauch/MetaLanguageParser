using MetaLanguageParser.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaLanguageParser.Parsing.Adapter;

namespace MetaLanguageParser.MetaCode.TypeWriter
{
    public abstract class TypeWriter // Setups IntendedTextWriter with Indent, and also closes the resource as well
	// Would be useful with a Field "writer"
    {
		protected System.CodeDom.Compiler.IndentedTextWriter writer;
		protected static System.Reflection.Assembly codeAsm;

        /// <summary>
        /// Will create a new TypeWriter Object of the given language. ##
        /// If found, compiles into assembly and executes custom TypeWriter; ##
        /// else, uses Fallback writer <see cref="Dummy"/>.class with simplyfied Methods
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        /// <remarks>Might look like much overhead, but it already existed for MetaCode, so it's mostly reused
        /// APPENDNOT: I should really look after smt to make this reuseable. Made it as equal as possible.</remarks>
		public static TypeWriter Factory(string lang)
        {
            Console.WriteLine("Checking TypeWriter directory...");
            string path = "MetaCode/TypeWriter";
            string libName = "TWLib";
            string lib = $"{libName}.dll";
            string libPath = path+"/"+lib;

            bool exists = File.Exists(libPath);
            FileInfo libFile = null;
            try { libFile = (exists) ? new FileInfo(libPath) : null; } catch (FileNotFoundException) { exists = false; }

            var fileDict = new Dictionary<string,string>(){{lang,""}};
            var dirFiles = Directory.EnumerateFiles(path);
            var fileList = new List<string>();
            string fileName = "";
            bool found = false;

            foreach (var item in dirFiles) {
                FileInfo fi = new FileInfo(item);
                Tokenize.Tokenizer.fixEncodingErrors(item);
                string file = fi.Name.Substring(0,fi.Name.Length-fi.Extension.Length);
                if(file.ToLower().Equals(lang)) fileName = file;
                if (fileDict.ContainsKey(file.ToLower())) {
                    found = true;
                    fileList.Add(File.ReadAllText(item));
                    if (exists && libFile.LastWriteTime < fi.LastWriteTime) exists = false;
                    break; // Since only one now
                }
            }

            MetaData.switchDictDirection(true);

            if (!found) {
                Console.WriteLine("WARNING: Could not find appropriate TypeWriter !");
                return new Dummy();
            }
            
            if (exists) {
                Console.WriteLine($"Loading last recent Library ({fileName})...");
                codeAsm = System.Reflection.Assembly.LoadFrom(libFile.FullName);
            } else {
                Console.WriteLine($"Compiling new {libName}...");
                //fileList.Add($"public class X {{ public static int cnt = {fileList.Count}; }}");
                codeAsm = Common.Reflection.Reflection.getAssembly(fileList, true, lib);
                if (codeAsm != null) {
                    File.Delete(libPath + "_old");
                    try {
                        File.Replace(lib, libPath, libPath + "_old");
                    } catch (IOException) {
                        File.Delete(libPath);
                        File.Move(lib, libPath);
                    }
                } else {
                    Console.WriteLine("Could not create Assembly!");
                    if (File.Exists(libPath)) {
                        Console.WriteLine("Loading Backup Lib !");
                        codeAsm = System.Reflection.Assembly.LoadFrom(libFile.FullName);
                    } else return null;
                }
            }

            return (TypeWriter) codeAsm.GetType("MetaLanguageParser.MetaCode.TypeWriter."+fileName).GetConstructor(Type.EmptyTypes).Invoke(null);
        }
        public static TypeWriter Factory2(string lang)
        {
            Console.WriteLine("Checking TypeWriter directory...");
            string path = "MetaCode/TypeWriter";
            string lib = "TWLib.dll";
            string libPath = path+"/"+lib;

            bool exists = File.Exists(libPath);
            FileInfo libFile = (exists) ? new FileInfo(libPath) : null;
            
            var dirFiles = Directory.EnumerateFiles(path);
            var fileList = new List<string>();
            string fileName = "";
            bool found = false;

            foreach (var item in dirFiles) {
                FileInfo fi = new FileInfo(item);
                Tokenize.Tokenizer.fixEncodingErrors(item);
                string file = fi.Name.Substring(0,fi.Name.Length-fi.Extension.Length);
                if (lang.Equals(file.ToLower())) {
                    found = true;
                    fileName = file;
                    fileList.Add(File.ReadAllText(item));
                    if (exists && libFile.LastWriteTime < fi.LastWriteTime) exists = false;
                    break; // Since only one now
                }
            }
            if (!found) {
                Console.WriteLine("WARNING: Could not find appropriate TypeWriter !");
                return new Dummy();
            }

            //if (exists && libFile.LastWriteTime < new FileInfo(linkPath).LastWriteTime) exists = false;
            if (exists) {
                Console.WriteLine($"Loading last recent Library ({fileName})...");
                codeAsm = System.Reflection.Assembly.LoadFrom(libFile.FullName);
            } else {
                Console.WriteLine("Compiling new TWLib...");
                codeAsm = Common.Reflection.Reflection.getAssembly(fileList, true, lib);
                if (codeAsm != null) {
                    File.Delete(libPath + "_old");
                    File.Replace(lib, libPath, libPath + "_old");
                } else {
                    Console.WriteLine("Could not create Assembly!");
                    return null;
                }
            }

            //fileDict.Remove("AddType");
            //kwDict.Add("§addType", AddType.parse);
            return (TypeWriter)codeAsm.GetType("MetaLanguageParser.MetaCode.TypeWriter." + fileName).GetConstructor(Type.EmptyTypes).Invoke(null);
            
        }

        #region Abstract Methods
        public string writeTypes(Dictionary<string, TypeData> typeDict){
			string result;
            StringWriter output = new StringWriter();
            writer = new System.CodeDom.Compiler.IndentedTextWriter(output, SYMBOL.Indent);
			
            var sb = new StringBuilder( );
			foreach (var item in typeDict) {
                Console.WriteLine($"Writing: {item.Value.debugDisplay()}...");
				writeType(item.Value);
            }
			result = output.ToString();
            writer?.Dispose();
            output?.Dispose();
			return result;
		}
		
		public abstract void writeType(TypeData data);
		//public abstract void writeField(FieldData data);
		//public abstract void writeProperty(PropData data);
		public abstract void writeCtor(MethodData data);
        public virtual void writeCctor(MethodData data) { }
		public virtual void writeDtor(MethodData data) { }
        public virtual void writeMethods(List<MethodData> dataList)
        {
            NewLine();
            foreach (var meth in dataList) {
                writer.WriteLine(meth.ToString(this));
            }
        }
        public abstract void writeMethod(MethodData data, bool isEntry);
        /** Write Shortcuts **/
        #endregion
            
        string seperator = " ";

        /// <summary>Search any match of keywords found in both <paramref name="mods"/> and <see cref="TypeData.modifiers"/>, and write the translated form.</summary>
        /// <param name="data"><see cref="TypeData"/> reference</param>
        /// <param name="mods">The array of strings to lookout for.</param>
        /// <param name="seperator">String to write inbetween hits.</param>
        /// <param name="addSep">Flag controling seperator output.</param>
        /// <returns>Value indicating whether or not strings were written,</returns>
        protected bool writeModifiers(TypeData data, string[] mods, string seperator, bool addSep = false)
        {
            var mod = data.getModifiers();
            string key;
            this.seperator = seperator;
            foreach (var item in mods) {
                if (!mod.Contains(item)) continue;
                if (addSep) writer.Write(seperator); else addSep = true;
                if (!KEYWORD.modDict.TryGetValue(item, out key)) key = $"__{item}__";
                writer.Write(key);
            }
            return addSep;
        }

        /// <summary>
        /// Write DIctTranslated form of given string.
        /// </summary>
        /// <param name="kw">Keyword to use</param>
        protected void writeModifier(string kw) => writeModifier(kw, "{0}");

        protected void writeModifier(string kw, string format)
        {
            string key;
            if (!KEYWORD.modDict.TryGetValue(kw, out key)) key = $"__{kw}__";
            writer.Write(string.Format(format, key));
        }

        /// <summary>
        /// Write the TypeMode (class, interface, struct, enum
        /// </summary>
        /// <param name="data"></param>
        /// <param name="addSep"></param>
        protected void writeMode(TypeData data, bool addSep)
        {
            var mode = data.getMode();
            if (addSep) writer.Write(seperator);
            writer.Write(MetaData.TypeModes[mode]);
        }

        protected void writeList<T>(List<T> list, string sep, bool startWithSep = false)
        {
            foreach (var item in list) {
                if (startWithSep) writer.Write(seperator); else startWithSep = true;
                writer.Write(item);
            }
        }



        protected void NewLine() => writer.WriteLine();
        protected void NewLine(int num)
        {
            for (int i = 0; i < num; i++) {
                writer.WriteLine();
            }
        }
        protected void WriteLine(string s) => writer.WriteLine(s);

        /// <summary> Write ' ', <see cref="SYMBOL.Block"/>, Increase Indent, Newline </summary>
        protected void OpenBlock()
        {
            writer.Write(' ');
            writer.Write(SYMBOL.Block);
            writer.Indent++;
            writer.WriteLine();
        }

        /// <summary> Decrease Indent, Write Newline, <see cref="SYMBOL.Block_Close"/>, Newline</summary>
        protected void CloseBlock()
        {
            writer.Indent--;
            writer.WriteLine();
            writer.Write(SYMBOL.Block_Close);
            writer.WriteLine();
        }

        /// <summary> Decrease Indent, Write Newline, <see cref="SYMBOL.Block_Close"/> + <paramref name="suffix"/>, Newline</summary>
        /// <param name="suffix">Appendix on BLOCK_CLOSE</param>
        protected void CloseBlock(string suffix)
        {
            writer.Indent--;
            writer.WriteLine();
            writer.Write(SYMBOL.Block_Close);
            writer.Write(suffix);
            writer.WriteLine();
        }

        static int cnt = 0;
        /// <summary>Will indent the given Block of Text onto the current depth, and writes it to the output.</summary>
        /// <param name="s"></param>
        protected void WriteBlock(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            string idntStr = "";
            switch (writer.Indent) {
                case 0: writer.Write(s); return;
                case 1: idntStr = SYMBOL.Indent; break;
                default:
                    //idntStr = SYMBOL.Indent.ConcatTimes(writer.Indent);
                    var sb = new StringBuilder();
                    for (int i = 0; i < writer.Indent; i++) sb.Append(SYMBOL.Indent);
                    idntStr = sb.ToString();
                    break;
            }
            // Apply the indent recursive to every code line
            //Program.printer($"CodeBlock_{cnt}_1-before", s);
            Program.printer($"CodeBlock_{cnt++}", s);
            var str = System.Text.RegularExpressions.Regex.Replace(s, "(^[\r\n]*(?:" + idntStr + ")*)", "${1}"+idntStr, System.Text.RegularExpressions.RegexOptions.Multiline);
            str = str.TrimStart(SYMBOL.Indent.ToCharArray());
            writer.Write(str);
            //Program.printer($"CodeBlock_{cnt++}_2-after", str);
        }
        
    }
}