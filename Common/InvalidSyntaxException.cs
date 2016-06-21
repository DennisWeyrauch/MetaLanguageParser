using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{

    /// <summary>
    /// The exception that is thrown when a syntax parser encounters invalid or unexpected tokens.
    /// </summary>
    [Serializable]
    public class InvalidSyntaxException : Exception
    {
        public int Position;
        public InvalidSyntaxException() : base("Code contains invalid Syntax.") { }
        public InvalidSyntaxException(string message) : base(message) { }
        public InvalidSyntaxException(string message, Exception inner) : base(message, inner) { }

        public InvalidSyntaxException addInnerException(Exception inner)
            => new InvalidSyntaxException(this.Message, inner);

        public InvalidSyntaxException(string message, int pos) : base($"{message} (Pos: {pos})") { Position = pos; }
        /// <summary></summary>
        /// <param name="kw">Current clause.</param>
        /// <param name="elem">Invalid token that was found.</param>
        /// <param name="pos">Position where the error occured.</param>
        public InvalidSyntaxException(string kw, string elem, int pos) : base($"[{kw}] Invalid token {elem} at pos {pos}.") { Position = pos; }
        /// <summary></summary>
        /// <param name="kw">Current clause.</param>
        /// <param name="elem">Invalid token that was found.</param>
        /// <param name="pos">Position where the error occured.</param>
        /// <param name="expected">Token-Type that was expected.</param>
        public InvalidSyntaxException(string kw, string elem, int pos, string expected) : base($"[{kw}] Invalid token {elem} at pos {pos} (Expected {expected}).") { Position = pos; }
        protected InvalidSyntaxException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
        /*
        public InvalidSyntaxException(string[] format, int pos, string[] args)
        {
            int arg = 0, idx = 0;
            var sb = new StringBuilder( );
            char[] ca;
            for (int i = 0; i < format.Length; i++) {
                ca = format[i].ToCharArray();
                idx = 0;
                idx = format[i].IndexOf('{', idx);
                format[i].
            }
        }//*/

        /// <summary>
        /// Merges multiple messages and throws them as one single <see cref="InvalidSyntaxException"/>.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="pos"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static void ThrowMultiple(string[] format, int pos, string[] args)
        {
            var sb = new StringBuilder(string.Format(format[0], args[0]));
            for (int i = 1; i < format.Length; i++) {
                sb.Append("\r\n");
                if (args[i] == null) sb.Append(format[i]);
                else sb.AppendFormat(format[i], args[i]);
            }
            throw new InvalidSyntaxException(sb.ToString(), pos);
        }

        internal static void ThrowMultiple2(string[] v1, int pos, string[][] v2)
        {
            throw new NotImplementedException();
        }

        internal static void ThrowMultiple3(string[] v, int pos, object[,] p)
        {
            throw new NotImplementedException();
        }

        public InvalidSyntaxException(string format, params string[] args) : base(string.Format(format, args)) { }
        public InvalidSyntaxException(string format, int pos, char arg) : this(string.Format(format, arg), pos) { }
        public InvalidSyntaxException(string format, int pos, string arg) : this(string.Format(format, arg), pos) { }
        public InvalidSyntaxException(string format, int pos, params string[] args) : this(string.Format(format, args), pos) { }
    }
}
