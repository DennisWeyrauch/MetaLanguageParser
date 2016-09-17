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
        /// <remarks>Might look like much overhead, but it already existed for MetaCode, so it's mostly reused</remarks>
		public static TypeWriter Factory(string lang){
            Console.WriteLine("Checking TypeWriter directory...");
            string path = "MetaCode/TypeWriter";
            string lib = "TWLib.dll";
            string libPath = path+"/"+lib;

            bool exists = File.Exists(libPath);
            FileInfo libFile = (exists) ? new FileInfo(libPath) : null;

            //var fileDict = readAnyFile(linkPath); // Use later for "Languages to compile".txt
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
            if(!found) {
                Console.WriteLine("WARNING: Could not find appropriate TypeWriter !");
                return new Dummy();
            }

            //if (exists && libFile.LastWriteTime < new FileInfo(linkPath).LastWriteTime) exists = false;
            if (exists) {
                Console.WriteLine($"Loading last recent Library ({fileName})...");
                codeAsm = System.Reflection.Assembly.LoadFrom(libFile.FullName);
            } else {
                Console.WriteLine("Compiling new TWLib...");
                //fileList.Add($"public class X {{ public static int cnt = {fileList.Count}; }}");
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
            return (TypeWriter) codeAsm.GetType("MetaLanguageParser.MetaCode.TypeWriter."+fileName).GetConstructor(Type.EmptyTypes).Invoke(null);

            /*foreach (var item in fileDict) {
                tempType = codeAsm.GetType("MetaLanguageParser.MetaCode.TypeWriter."+item.Key);
                var del = tempType.GetMethod("parse", Common.Reflection.Reflection.LookupAll).CreateDelegate(typeof(CodeDel));
                kwDict.Add(item.Value, (CodeDel) del);
                //kwDict.Add(item.Value, (CodeDel)(codeAsm.GetType("MetaLanguageParser.MetaCode." + item.Key)).GetMethod("parse", Common.Reflection.Reflection.LookupAll).CreateDelegate(typeof(CodeDel)));
            }//*/
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


        Dictionary<string,string> modifiers = new Dictionary<string,string>();
        string seperator = " ";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mods"></param>
        /// <param name="seperator"></param>
        /// <returns>Value indicating whether or not strings were written,</returns>
        protected bool writeModifiers(TypeData data, string[] mods, string seperator, bool first = true)
        {
            var mod = data.getModifiers();
            string key;
            this.seperator = seperator;
            foreach (var item in mods) {
                if (!mod.Contains(item)) continue;
                if (first) first = false; else writer.Write(seperator);
                if (!modifiers.TryGetValue(item, out key)) key = $"__{item}__";
                writer.Write(key);
            }
            return first;
        }

        protected void writeModifier(string kw)
        {
            string key;
            if (!modifiers.TryGetValue(kw, out key)) key = $"__{kw}__";
            writer.Write(key);
        }

        protected void writeModifier(string kw, string format)
        {
            string key;
            if (!modifiers.TryGetValue(kw, out key)) key = $"__{kw}__";
            writer.Write(string.Format(format, key));
        }

        protected void writeMode(TypeData data, bool state)
        {
            var mode = data.getMode();
            var dict = MetaData.TypeModes;
            if (state) writer.Write(seperator);
            writer.Write(dict[mode]);
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

		
        protected void WriteLineInc(string s)
        {
            writer.Write(s);
            writer.Indent++;
            writer.WriteLine();
        }
        protected void WriteLineDec(string s)
        {
            writer.Write(s);
            writer.Indent--;
            writer.WriteLine();
        }
    }
}