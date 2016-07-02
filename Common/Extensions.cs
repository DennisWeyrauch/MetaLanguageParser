using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{

    public static class Extensions
    {
        #region ExportText
        /// <summary>
        /// Creates or overwrites File at '<paramref name="path"/>' to write into.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        public static void WriteText(this string str, string path) => System.IO.File.WriteAllText(path, str);
        /// <summary>
        /// Creates or overwrites File at '<paramref name="path"/>' to write into.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        public static void WriteLine(this string str, string path) => (str + "\r\n").WriteText(path);

        /// <summary>
        /// Creates or opens File at '<paramref name="path"/>' and appends the given text.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        public static void AppendText(this string str, string path) => System.IO.File.AppendAllText(path, str);
        /// <summary>
        /// Creates or opens File at '<paramref name="path"/>' and appends the given text.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        public static void AppendLine(this string str, string path) => (str + "\r\n").AppendText(path);
        #endregion
        #region AutoLogger
        const string DateTime_Format = @"yyyy\/MM\/dd_HH-mm-ss";

        /// <summary>
        /// Attempts to open the logFile and append the given logInformation.<para/>
        /// Result: 
        /// &lt;yyyy-mm-dd HH:mm:ss&gt;: $text${newline}
        /// </summary>
        /// <param name="text">Text to write into log</param>
        /// <param name="logFile">Specifies logFile to write into. Default: 'logger.log'</param>
        /// <param name="multiLine">If false: adds Timestamp in front of Line</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static void logData(string text, string logFile = "logger.log", bool multiLine = false)
        {
            lock (objLock) {
                FileMode fm = (File.Exists(logFile)) ? FileMode.Append : FileMode.Create;
                FileStream logStream = null;
                try {
                    logStream = new FileStream(logFile, fm);
                    using (StreamWriter sw = new StreamWriter(logStream)) {
                        if (multiLine) sw.WriteLine(text);
                        else {
                            StringBuilder sb = new StringBuilder()
                            .Append(DateTime.Now.ToString(DateTime_Format))
                            .Append($": {text}");
                            sw.WriteLine(sb.ToString());
                        }
                    }
                } finally {
                    if (logStream != null) logStream.Dispose();
                }
            }
        }
        private static object objLock = new object();
        public static void logData(this string text) => logData(text, "logger.log", false);
        #endregion

        #region IEnumerable<T> to Delimited string
        // Use: .ToDelimitedString(x => x.Property1 + "-" + x.Property2)

        static string _listSep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        public static string ToDelimitedString<T>(this IEnumerable<T> source)
            => source.ToDelimitedString(x => x.ToString(), _listSep);

        public static string ToDelimitedString<T>(this IEnumerable<T> source, Func<T, string> converter)
            => source.ToDelimitedString(converter, _listSep);

        public static string ToDelimitedString<T>(this IEnumerable<T> source, string separator)
            => source.ToDelimitedString(x => x.ToString(), separator);

        public static string ToDelimitedString<T>(
            this IEnumerable<T> source, Func<T, string> converter, string separator)
        {
            return string.Join(separator, source.Select(converter).ToArray());
        }

        #endregion
        #region gh

        /// <summary> Concats an array of strings into one big Regex-Alternative </summary>
        /// <param name="op">The string array to concatinate</param>
        /// <returns>A string of the form "(elem0|elem1|...)"</returns>
        public static string concatArray(string[] op, bool escape = true)
        {
            if (escape) return "(" + string.Join("|", op.Select(t => Regex.Escape(t))) + ")";
            return string.Join("|", op);
        }

        /// <summary> Concats an verbatim string constisting of elements seperated with <paramref name="sep"/>. Will recognize groups enclosed in ""</summary>
        /// <param name="verb">The string to concatinate</param>
        /// <param name="sep">Seperator to use. Default [ \t\r\n]</param>
        /// <returns>A string of the form "elem0|elem1|..."</returns>
        public static string concatVerbatim(string verb, string sep = @"\s")
        {
            return Regex.Replace(verb, @"(?:""([^""]*)"")?(?:" + sep + @")+", @"$1|");
        }

        /// <summary>
        /// Helper function to retrieve all matches of <paramref name="pattern"/> in the given input
        /// </summary>
        /// <param name="input">The input to parse</param>
        /// <param name="pattern">The pattern to use</param>
        /// <returns>A List containing all matches in order of occurence (May contain duplicates)</returns>
        public static List<string> matchHelper(string input, string pattern)
        {
            var matches = Regex.Matches(input, pattern).Cast<Match>();
            var sa = new List<string>();
            foreach (var item in from m in matches select m.Groups[1].Value) {
                sa.Add(item);
            }
            return sa;
        }
        #endregion

        #region Extension methods
        /// <summary> ShortCut Extension for "string.IsNullOrEmpty(this)" </summary>
        /// <returns>(s == null || s?.Equals(""))</returns>
        public static bool IsNOE(this string s) => string.IsNullOrEmpty(s);
        /// <summary> ShortCut Extension for "~(string.IsNullOrEmpty(this))" </summary>
        /// <returns>(s != null &amp;&amp; !s?.Equals(""))</returns>
        public static bool IsNotNOE(this string s) => !string.IsNullOrEmpty(s);

        /// <summary> Extension method for <see cref="List{T}"/>.Add(<see cref="IEnumerable{T}"/>).
        /// This allows the inclusion of other lists in the Initializer</summary>
        /// <param name="list">Extension parameter</param>
        /// <param name="collection">Collection to add</param>
        public static void Add(this List<string> list, IEnumerable<string> collection)
                 => list.AddRange(collection);
        /// <summary>Extension method to allow concatination of <see cref="List{T}.Add(T)"/></summary>
        /// <param name="list">Extension parameter</param>
        /// <param name="item">Item to add</param>
        /// <returns>The list after item has been added</returns>
        public static List<string> Add2(this List<string> list, string item) { list.Add(item); return list; }
        /// <summary>Extension method to allow concatination of <see cref="List{T}.AddRange(IEnumerable{T})"/></summary>
        /// <param name="list">Extension parameter</param>
        /// <param name="collection">Collection to add</param>
        /// <returns>The list after the collection has been added</returns>
        public static List<string> AddRange2(this List<string> list, IEnumerable<string> collection)
        { list.AddRange(collection); return list; }

        public static System.Boolean IsNumeric(this System.Object Expression)
        {
            if (Expression == null || Expression is DateTime)
                return false;

            if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;

            try {
                if (Expression is string) {
                    if (Expression.Equals("true") || Expression.Equals("false")) return true;
                    Double.Parse(Expression as string);
                } else
                    Double.Parse(Expression.ToString());
                return true;
            } catch { } // just dismiss errors but return false
            return false;
        }

        /// <summary>
        /// Concatenates the specified instance of <see cref="System.String"/> with itself as many times as specified in <paramref name="times"/>.
        /// </summary>
        /// <param name="str">The first string to concatenate</param>
        /// <param name="times"></param>
        /// <returns></returns>
        public static string ConcatTimes(this string str, int times)
        {
            var sb = new StringBuilder(str);
            for (int i = 1; i < times; i++) sb.Append(str);
            return sb.ToString();
        }

    #endregion

    /// <summary>
    /// Not yet tested.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ex"></param>
    /// <param name="inner"></param>
    public static void addInnerException<T>(this T ex, Exception inner) where T : Exception, new()
        {
            var ci = typeof(T).GetConstructor(new[] {typeof(string), typeof(Exception)});
            ex = (T)ci.Invoke(ex.Message, new[] { inner });
        }
        
        /// <summary>
        /// Find the first occurence of any char of <paramref name="carr"/> in <paramref name="s"/>.
        /// </summary>
        /// <param name="s">Extension Parameter</param>
        /// <param name="carr"></param>
        /// <returns></returns>
        public static int IndexOfAny(this string s, params char[] carr) => s.IndexOfAny(carr);

        /// <summary>
        /// Find the first occurence of any char of <paramref name="chars"/> in <paramref name="s"/>.
        /// </summary>
        /// <param name="s">Extension Parameter</param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static int IndexOfAny(this string s, string chars) => s.IndexOfAny(chars.ToCharArray());

        public static bool EqualsChar(this string s, char c)
        {
            //if (s.Length == 1) return s.ToCharArray()[0] == c;
            //return s.Equals(c.ToString());
            return (s.Length == 1) ? s.ToCharArray()[0] == c : false;
        }

        public static T AsEnum<T>(this object ct) where T : struct
        {
            return (T)System.Enum.Parse(typeof(T), ct.ToString(), false);
        }

        //public static implicit operator Array(List<T> list)  => list.ToArray();

    }/*
    public class CustomOperators
    {

        public static implicit operator char (string s)
            => new char();
    }//*/

    public static class Asserter
    {
        public static void AssertNotNull(object obj, string name)
        {
            if (obj == null) throw new ArgumentNullException(name);
        }
    }
}
