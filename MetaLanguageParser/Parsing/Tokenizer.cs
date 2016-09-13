using System; // Console
using System.Collections.Generic;
using System.IO; // File
using System.Linq; // string.Select()
using System.Text.RegularExpressions; // Regex, RegexOptions, Match
using ListWalker = Common.ListWalker;

namespace MetaLanguageParser.Tokenize
{
    public abstract class Tokenizer
    {
        // From: http://www.codeproject.com/Articles/371232/Escaping-in-Csharp-characters-strings-string-forma
        
        public Tokenizer() { }

        public abstract string createPattern();

        public ListWalker execute(string path, bool printList = false)
        {
            string pattern = createPattern();
            //string path = @"..\..\Common\Reflection_partial.cs"; // enter your path to the C# file to parse
            string name = path.Substring(path.LastIndexOf('\\')+1);
            string input = File.ReadAllText(path);
            fixEncodingErrors(path, ref input);
            input = Common.Reflection_partial.escapeCode(input, true);
            var tokens = new ListWalker();
            var matches = Regex.Matches(input, pattern, RegexOptions.Singleline|RegexOptions.Compiled).Cast<Match>();
            if (printList) {
                using (StreamWriter sw = new StreamWriter(new FileStream($"logs\\log_{name}", FileMode.Create))) {
                    foreach (var token in from m in matches select m.Groups[1].Value) {
                        //Console.Write(" {0}", token);
                        sw.Write($" {token}");
                        tokens.Add(token);
                        if ("{};".Contains(token)) {
                            //Console.WriteLine();
                            sw.WriteLine();
                        }
                    }
                }
            } else {
                foreach (var token in from m in matches select m.Groups[1].Value) {
                    tokens.Add(token);
                }
            }
            //Console.Read();
            return tokens;
        }
        /// <summary>
        /// Silent Tokenizer for Code Fragments. Used by the parser for interpolated Strings.
        /// </summary>
        /// <param name="code">The code to tokenize.</param>
        /// <param name="embedded">True for single line expressions.</param>
        /// <returns></returns>
        /// <remarks>With silent I mean without Console or File output</remarks>
        public static ListWalker TokenizeCode(string code, enumType et, bool embedded = false)
        {
            var Pattern = createTokenizer(et).createPattern();
            RegexOptions regOp = RegexOptions.Compiled | ((embedded) ? RegexOptions.Singleline : RegexOptions.Multiline);
            string input = (embedded)
                ? Common.Reflection_partial.escapeEmbedded(code) // Only remove BlockComments.
                : Common.Reflection_partial.escapeCode(code, true);
            var tokens = new ListWalker();
            input = Regex.Replace(input, "\x81\x98", "Åò");
            var matches = Regex.Matches(input, Pattern, regOp).Cast<Match>();
            foreach (var token in from m in matches select m.Groups[1].Value) {
                tokens.Add(token);
            }
            return tokens;
        }
        /// <summary>
        /// Silent Tokenizer for Code Fragments. Used by the parser for interpolated Strings.
        /// </summary>
        /// <param name="path">The file to tokenize.</param>
        /// <param name="singleLine">True for single line expressions.</param>
        /// <returns></returns>
        /// <remarks>With silent I mean without Console or File output</remarks>
        public static ListWalker TokenizeFile(string path, enumType et, bool singleLine)
        {
            var Pattern = createTokenizer(et).createPattern();

            string input = File.ReadAllText(path);
            fixEncodingErrors(path, ref input);
            input = Common.Reflection_partial.escapeCode(input, true);
            RegexOptions regOp = RegexOptions.Compiled | ((singleLine) ? RegexOptions.Singleline : RegexOptions.Multiline);
            input = (singleLine)
                ? Common.Reflection_partial.escapeEmbedded(input) // Only remove BlockComments.
                : Common.Reflection_partial.escapeCode(input, true);
            var tokens = new ListWalker();
            var matches = Regex.Matches(input, Pattern, regOp).Cast<Match>();
            foreach (var token in from m in matches select m.Groups[1].Value) {
                tokens.Add(token);
            }
            return tokens;
        }

        /// <summary>
        /// Fixing encoding issues of the Paragraph Symbol (Åò) between ANSI and UTF-8
        /// </summary>
        /// <param name="path">The path to write back changes to.</param>
        public static void fixEncodingErrors(string path)
        {
            string input = File.ReadAllText(path);
            fixEncodingErrors(path, ref input);
        }

        /// <summary>
        /// Fixing encoding issues of the Paragraph Symbol (Åò) between ANSI and UTF-8
        /// </summary>
        /// <param name="path">The path to write back changes to.</param>
        /// <param name="input">The input to fix</param>
        public static void fixEncodingErrors(string path, ref string input)
        {
            /*var sb = new System.Text.StringBuilder( );
            foreach (var item in input) {
                if (item == '\n' || item == '\r') sb.AppendLine();
                else sb.Append((int)item).Append("  ");
            }
            File.AppendAllText("tokenChar.log", "\r\n\r\n" + sb.ToString());//*/
            if (Regex.IsMatch(input, "\xFFFD+")) {
                var bytes = File.ReadAllBytes(path);
                var newBytes = bytes;
                for (int i = 0; i < bytes.Length - 1; i++) {
                    try {
                        if (bytes[i] == 0x81 && (i + 1 < bytes.Length) && bytes[i + 1] == 0x98) { // Paragraph Symbol: ANSI -> UTF-8
                            newBytes[i] = 0xC2;
                            newBytes[++i] = 0xA7;
                        }
                    } catch (IndexOutOfRangeException) { break; }
                }
                File.WriteAllBytes(path, newBytes);
                input = File.ReadAllText(path);
            }
        }

        public enum enumType
        {
            /// <summary>C# code</summary>
            CSharp,
            /// <summary></summary>
            MetaParser,
            /// <summary>Meta-To-Dest File</summary>
            MetaType,
            /// <summary>Meta-Definition File</summary>
            MetaDef
        }
        static Dictionary<enumType, System.Reflection.ConstructorInfo> enumDict = new Dictionary<enumType, System.Reflection.ConstructorInfo>() {
            {enumType.CSharp,  typeof(CSharpTokenizer).GetConstructor(Type.EmptyTypes)},
            {enumType.MetaParser,  typeof(MetaTokenizer).GetConstructor(Type.EmptyTypes)},
            {enumType.MetaType,  typeof(MetaCodeTokenizer).GetConstructor(Type.EmptyTypes)},
            {enumType.MetaDef,  typeof(MetaDefTokenizer).GetConstructor(Type.EmptyTypes)},
        };

        public static Tokenizer createTokenizer(enumType type) => (Tokenizer) enumDict[type].Invoke(null);
    }

    class CSharpTokenizer : Tokenizer
    {
        static string _pattern;
        public string Pattern => _pattern ?? createPattern();
        const string hex = @"[0-9a-fA-F]";
        const string euler = @"(?:[eE][-+]?\d+)";
        string strlit  = @"""(?:\\u[0-9a-fA-F]{4}|\\U[0-9a-fA-F]{8}|\\x[0-9a-fA-F]{1,4}|\\.|[^""])*""";
        /**/
        string verlit  = @"@""(?:""""|[^""])*"""; /*/// or: "@\"(?:\"\"|[^\"])*\""
        string verlit  = @"@""(?:""""|[\s]+|[^""])*"""; //*/
        string charlit = @"'(?:\\u[0-9a-fA-F]{4}|\\x[0-9a-fA-F]{1,4}|\\.|[^'])'";
        string hexlit  = @"0[xX][0-9a-fA-F]+[ulUL]?";
        string number1 = @"(?:\d*\.\d+)(?:[eE][-+]?\d+)?[fdmFDM]?";
        string number2 = @"\d+(?:[ulUL]?|(?:[eE][-+]?\d+)[fdmFDM]?|[fdmFDM])";
        string ident   = @"@?(?:\\u[0-9a-fA-F]{4}|\\U[0-9a-fA-F]{8}|\w)+";
        string[] op3   = new string[] {"<<="};
        string[] op2   = new string[] {"!=","%=","&&","&=","*=","++","+=","--","-=","/=",
                               "::","<<","<=","==","=>","??","^=","|=","||"};
        string rest = @"\S";


        public override string createPattern()
        {
            if (_pattern != null) return _pattern;

            string skip = @"(?:"+ string.Join("|", new string[]
            {
                @"[#].*?\n",                                     // C# pre processor line
                @"//.*?\n",                                      // C# single line comment
                @"/[*][\s\S]*?[*]/",                             // C# block comment
                @"\s",                                           // white-space
            }) + @")*";
            _pattern = skip + "(" + string.Join("|", new string[]
            {
                strlit,                                          // C# string literal
                verlit,                                          // C# verbatim literal
                charlit,                                         // C# character literal
                hexlit,                                          // C# hex number literal
                number1,                                         // C# real literal
                number2,                                         // C# integer or real literal
                ident,                                           // C# identifiers
                string.Join("|",op3.Select(t=>Regex.Escape(t))), // C# three-letter operator
                string.Join("|",op2.Select(t=>Regex.Escape(t))), // C# two-letter operator
                rest,                                            // C# one-letter operator and any other one char
            }) + @")" + skip;
            return _pattern;
        }
    }
    /// <summary>
    /// For parsing MetaCode Files
    /// </summary>
    class MetaTokenizer : Tokenizer
    {
        static string _pattern;

        public string Pattern => _pattern ?? createPattern();
        string strlit  = @"""(?:\\u[0-9a-fA-F]{4}|\\U[0-9a-fA-F]{8}|\\x[0-9a-fA-F]{1,4}|\\.|[^""])*""";
        /**/
        string verlit  = @"@""(?:""""|[^""])*"""; /*/// or: "@\"(?:\"\"|[^\"])*\""
        string verlit  = @"@""(?:""""|[\s]+|[^""])*"""; //*/
        string charlit = @"'(?:\\u[0-9a-fA-F]{4}|\\x[0-9a-fA-F]{1,4}|\\.|[^'])'";
        string hexlit  = @"0[xX][0-9a-fA-F]+[ulUL]?";
        string number1 = @"(?:\d*\.\d+)(?:[eE][-+]?\d+)?[fdmFDM]?";
        string number2 = @"\d+(?:[ulUL]?|(?:[eE][-+]?\d+)[fdmFDM]?|[fdmFDM])";
        string ident   = @"@?\Åò?(?:\\u[0-9a-fA-F]{4}|\\U[0-9a-fA-F]{8}|\w)+";
        string[] op3   = new string[] {"<<="};
        string[] op2   = new string[] {"!=","%=","&&","&=","*=","++","+=","--","-=","/=",
                               "$$", "::","<<","<=","==","=>","??","^=","|=","||"};
        string rest = @"\S";


        public override string createPattern()
        {
            if (_pattern != null) return _pattern;

            string skip = @"(?:"+ string.Join("|", new string[]
            {
                @"[#].*?\n",                                     // C# pre processor line
                @"//.*?\n",                                      // C# single line comment
                @"/[*][\s\S]*?[*]/",                             // C# block comment
                @"\s",                                           // white-space
            }) + @")*";
            _pattern = skip + "(" + string.Join("|", new string[]
            {
                strlit,                                          // C# string literal
                verlit,                                          // C# verbatim literal
                charlit,                                         // C# character literal
                hexlit,                                          // C# hex number literal
                number1,                                         // C# real literal
                number2,                                         // C# integer or real literal
                ident,                                           // C# identifiers
                string.Join("|",op3.Select(t=>Regex.Escape(t))), // C# three-letter operator
                string.Join("|",op2.Select(t=>Regex.Escape(t))), // C# two-letter operator
                rest,                                            // C# one-letter operator and any other one char
            }) + @")" + skip;
            return _pattern;
        }
    }
    /// <summary>
    /// For parsing Meta-To-DestCode Files
    /// </summary>
    class MetaCodeTokenizer : Tokenizer
    {
        static string _pattern;

        public string Pattern => _pattern ?? createPattern();

        string ident   = @"@?(?:\\u[0-9a-fA-F]{4}|\\U[0-9a-fA-F]{8}|\w)+";
        string[] op2   = new string[] {"$$", "::"};
        //string rest = @"\S";

        public override string createPattern()
        {
            if (_pattern != null) return _pattern;

            /*
            string skip = @"(?:"+ string.Join("|", new string[]
            {
                //@"//.*?\n",                                      // C# single line comment
                @"/[*][\s\S]*?[*]/",                             // C# block comment
               // @"\s",                                           // white-space
            }) + @")*";
            _pattern = skip + "(" + string.Join("|", new string[]
            {
                ident,                                           // C# identifiers
                string.Join("|",op2.Select(t=>Regex.Escape(t))), // C# two-letter operator
                rest,                                            // C# one-letter operator and any other one char
            }) + @")" + skip;//*/
            _pattern = @"(\$\$|Åò\w+|[\r\n]+|(?:[^$Åò\r\n]+))";
            return _pattern;
        }
    }
    /// <summary>
    /// For parsing Meta-Definition Files
    /// </summary>
    class MetaDefTokenizer : Tokenizer
    {
        static string _pattern;

        public string Pattern => _pattern ?? createPattern();

        string ident   = @"@?Åò?(?:\\u[0-9a-fA-F]{4}|\\U[0-9a-fA-F]{8}|\w)+";
        string[] op2   = new string[] {"$$", "::"};
        string rest = @"\S";

        public override string createPattern()
        {
            if (_pattern != null) return _pattern;


            string skip = @"(?:"+ string.Join("|", new string[]
            {
                //@"//.*?\n",                                      // C# single line comment
                @"/[*][\s\S]*?[*]/",                             // C# block comment
                @"\s",                                           // white-space
            }) + @")*";
            _pattern = skip + "(" + string.Join("|", new string[]
            {
                ident,                                           // C# identifiers
                string.Join("|",op2.Select(t=>Regex.Escape(t))), // C# two-letter operator
                rest,                                            // C# one-letter operator and any other one char
            }) + @")" + skip;
            //_pattern = @"(?:\s)|(\$\$|::|\w+|\S+?)";
            return _pattern;
        }
    }

}