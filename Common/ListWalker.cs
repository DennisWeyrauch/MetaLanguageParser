using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Extensions;

namespace Common
{
    /// <summary>
    /// Special <see cref="IOrderedEnumerable{TElement}"/> wrapper of <see cref="List{T}"/> (T is string), with various methods for navigation and/or equivalence (both char and string)
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Token[{Index}/{Count}] = {this[Index]}")]
    public class ListWalker : List<string>, IOrderedEnumerable<string>
    {
        static int instanceId = 0;
        public ListWalker(bool append = true) {
            appendLog = append; // is in Region ErrorPrinter
            if (instanceId > 0) {
                logName += string.Format("_{0:00}", instanceId);//string.Format("{0}_{1:00}.{2}",logName, instanceId, logSuffix);
            }
            instanceId++;
        }
        public ListWalker(List<string> list, bool append = true) : this(append)
        {
            this.AddRange(list);
        }
        static ListWalker()
        {
            deleteLogs();
        }

        /// <summary>The current Position in the List. NOT readonly (see remarks)</summary>
        /// <remarks>
        /// Should be a property, but isn't so that one can use it as ref-parameter, and manipulate it via ++/--
        /// This will avoid unneccessary calls to getCurrent in various contexts, AND it's smaller
        /// (ldarg.1[1], ldind.ref[1] &lt;&lt;vs.&gt;&gt; ldloc.x (list)[1-2], call getCurrent[5])
        /// </remarks>
        public int Index;// { get; internal set; }
#if true
        bool addEOF = true;
        bool hasEOF = false;
        public new int Count
        {
            get
            {
                if (addEOF | hasEOF) {
                    if (!hasEOF) {
                        //var i = this.FindLastIndex( s => s.Equals("EOF"));
                        if (this.FindLastIndex(s => s.Equals("EOF")) == -1) this.Add("EOF");
                        hasEOF = true;
                    }
                    return base.Count - 1;
                }
                return base.Count;
            }
        }
#endif
        #region Navigation Methods
        public string getCurrent() => this[Index];
        /// <summary> 
        /// this[Index][0] ---Converts the current token to a char[] and returns the first element. 
        /// </summary>
        /// <returns></returns>
        public char getCurrentAsChar() => this[Index][0];
        public char getNextAsChar() => this[++Index][0];
        public string getNext() => this[++Index];
        public string peekNext() => this[Index + 1];
        /// <summary>Return "<see cref="this[Index]"/> == <paramref name="c"/>".</summary>
        public bool isCurrent(char c) => this[Index].EqualsChar(c);
        /// <summary>Return "<see cref="this[Index]"/> == <paramref name="s"/>".</summary>
        public bool isCurrent(string s) => this[Index].Equals(s);
        /// <summary>Return "<see cref="this[Index+1]"/> == <paramref name="c"/>".</summary>
        public bool peekNext(char c) => this[Index + 1].EqualsChar(c);
        /// <summary>Return "<see cref="this[Index+1]"/> == <paramref name="s"/>".</summary>
        public bool peekNext(string s) => this[Index + 1].Equals(s);

        /// <summary>Increment index and check if element == param.</summary>
        public bool nextIs(char c) => this.getNextAsChar() == c;//this.getNext().EqualsChar(c);
        /// <summary>Increment index and check if element == param.</summary>
        public bool nextIs(string s) => this.getNext().Equals(s);
        /// <summary>Increment index and check if element != param.</summary>
        public bool nextNot(char c) => !nextIs(c);
        /// <summary>Increment index and check if element != param.</summary>
        public bool nextNot(string s) => !nextIs(s);
        //public bool nextNot2(char c) => !this.getNext().EqualsChar(c);
        //public bool nextNot(string S) => !this.getNext().Equals(s);
        #endregion
        #region While Conditionals
        /// <summary>
        /// Increment Index, assign token, and return '<paramref name="para"/> == this[<see cref="Index"/>]'.
        /// <para/>
        /// (token = <see cref="getNext()"/>).Equals(<paramref name="para"/>);
        /// </summary>
        /// <param name="token">The object to fill.</param>
        /// <param name="para">The object to check against.</param>
        /// <returns></returns>
        public bool whileTrue(ref string token, char para) => (token = this.getNext()).EqualsChar(para);
        /// <summary>
        /// Increment Index, assign token, and return '<paramref name="para"/> == this[<see cref="Index"/>]'.
        /// <para/>
        /// (token = <see cref="getNext()"/>).Equals(<paramref name="para"/>);
        /// </summary>
        /// <param name="token">The object to fill.</param>
        /// <param name="para">The object to check against.</param>
        /// <returns></returns>
        public bool whileTrue(ref string token, string para) => (token = this.getNext()).Equals(para);

        /// <summary>
        /// Increment Index, assign token, and return '<paramref name="para"/> != this[<see cref="Index"/>]'.
        /// <para/>
        /// !(token = <see cref="getNext()"/>).Equals(<paramref name="para"/>);
        /// </summary>
        /// <param name="token">The object to fill.</param>
        /// <param name="para">The object to check against.</param>
        /// <returns></returns>
        public bool whileNot(ref string token, char para) => !whileTrue(ref token, para);
        /// <summary>
        /// Increment Index, assign token, and return '<paramref name="para"/> != this[<see cref="Index"/>]'.
        /// <para/>
        /// !(token = <see cref="getNext()"/>).Equals(<paramref name="para"/>);
        /// </summary>
        /// <param name="token">The object to fill.</param>
        /// <param name="para">The object to check against.</param>
        /// <returns></returns>
        public bool whileNot(ref string token, string para) => !whileTrue(ref token, para);


        /// <summary>
        /// Increment Index, assign token, and return 'NOT(token contains <paramref name="para"/>)'.
        /// <para/>
        /// (token = <see cref="getNext()"/>).IndexOf(<paramref name="para"/>) == -1;
        /// </summary>
        /// <param name="token">The object to fill.</param>
        /// <param name="para">The object to check against.</param>
        /// <returns></returns>
        public bool hasNot(ref string token, char para) => (token = getNext()).IndexOf(para) == -1;


        #endregion
        #region Skip Methods
        /// <summary>Will stop on the first occurence of <paramref name="c"/>.</summary>
        public void skipUntil(char c) {
            while (true) {
                if (isEOF()) return;
                if (isCurrent(c)) return;
                Index++;
            }
        }
        /// <summary>Will stop on the first occurence of <paramref name="s"/>.</summary>
        public void skipUntil(string s)
        {
            while (true) {
                if (isEOF()) return;
                if (isCurrent(s)) return;
                Index++;
            }
        }

        public void skipBalanced(char open, char close)
        {
            int cnt = 1;
            string elem = "";
#warning CUSTOM:: Parameterlist-Seperator, Closure-Opening/Terminator
            while (!isEOF() && cnt > 0) {
                elem = getNext();
                if (elem.EqualsChar(open)) cnt++;
                else if (elem.EqualsChar(close)) cnt--;
                if(elem.EqualsChar(';') || elem.StartsWith("§")) break;
            }

        }

        #endregion
        #region Assert Methods

        #region CharMethods

        /// <summary>
        /// Throws <see cref="InvalidSyntaxException"/> when current element is not <paramref name="c"/>.
        /// </summary>
        /// <param name="c"></param>
        public void assertNoInc(char c)
        {
            if (!isCurrent(c)) throw new InvalidSyntaxException();
        }
        /// <summary>
        /// Throws <see cref="InvalidSyntaxException"/> when current element is not <paramref name="c"/>.
        /// Otherwise increments the Index and returns the next Element.
        /// </summary>
        /// <param name="c"></param>
        public string assert(char c)
        {
            if (!isCurrent(c)) throw new InvalidSyntaxException();
            return getNext();
        }
        /// <summary>
        /// Throws <see cref="InvalidSyntaxException"/> when current element is not <paramref name="c"/>.
        /// Otherwise increments the Index and returns the instance to allow chaining.
        /// </summary>
        /// <param name="c"></param>
        public ListWalker assertC(char c)
        {
            if (!isCurrent(c)) throw new InvalidSyntaxException();
            Index++;
            return this;
        }
        /// <summary>
        /// Throws <see cref="InvalidSyntaxException"/> if NEXT element is not <paramref name="c"/>.
        /// Otherwise returns that Element (pos+1 relative to caller).
        /// </summary>
        /// <param name="c"></param>
        public string assertNext(char c)
        {
            if (nextNot(c)) throw new InvalidSyntaxException();
            return getCurrent();
        }

        #endregion
        #region StringMethods
        /// <summary>
        /// Throws <see cref="InvalidSyntaxException"/> when current element is not <paramref name="s"/>.
        /// </summary>
        /// <param name="s"></param>
        public void assertNoInc(string s)
        {
            if (!isCurrent(s)) throw new InvalidSyntaxException();
        }
        /// <summary>
        /// Throws <see cref="InvalidSyntaxException"/> when current element is not <paramref name="s"/>.
        /// Otherwise increments the Index and returns the next Element.
        /// </summary>
        /// <param name="s"></param>
        public string assert(string s)
        {
            if (!isCurrent(s)) throw new InvalidSyntaxException();
            return getNext();
        }
        /// <summary>
        /// Throws <see cref="InvalidSyntaxException"/> when current element is not <paramref name="s"/>.
        /// Otherwise increments the Index and returns the instance to allow chaining.
        /// </summary>
        /// <param name="s"></param>
        public ListWalker assertC(string s)
        {
            if (!isCurrent(s)) throw new InvalidSyntaxException();
            Index++;
            return this;
        }
        /// <summary>
        /// Throws <see cref="InvalidSyntaxException"/> when current element is not <paramref name="s"/>.
        /// Otherwise increments the Index and returns the instance to allow chaining.
        /// </summary>
        /// <param name="s"></param>
        public ListWalker assertMulti(params string[] s)
        {
            int len = s.Length;
            s = s.Reverse().ToArray();
            while(len != 0) {
                //if (!isCurrent(s[len])) throw new InvalidSyntaxException();
                if (!this[Index].Equals(s[len])) throw new InvalidSyntaxException();
                len--;
                Index++;
            }
            return this;
        }
        /// <summary>
        /// Throws <see cref="InvalidSyntaxException"/> if NEXT element is not <paramref name="s"/>.
        /// Otherwise returns that Element (pos+1 relative to caller).
        /// </summary>
        /// <param name="s"></param>
        public string assertNext(string s)
        {
            if (nextNot(s)) throw new InvalidSyntaxException();
            return getCurrent();
        }
        #endregion


        /// <summary> Returns if this[Index] == '}'</summary><returns>True/Fals</returns>
        public bool isClosure() => this[Index].Equals("}");
        /// <summary> Returns if this[Index] == EOF</summary><returns>True/False</returns>
        public bool isEOF()/*/ => this[Index].Equals("EOF");/*/
        {
            bool retVal = true;
            try {
                retVal = this[Index].Equals("EOF");
            } catch (ArgumentOutOfRangeException) {
                //retVal = this[Index].Equals("EOF");
            }
            return retVal;
        }//*/
        /// <summary> Returns <see cref="isClosure"/> || <see cref="isEOF"/> </summary><returns>True/False</returns>
        public bool isAtEnd() => isEOF() || isClosure();
        /// <summary> Returns <see cref="isClosure"/> || <see cref="isEOF"/> || <see cref="this[int]"/>.Equals(<paramref name="str"/>) </summary><returns>True/False</returns>
        public bool isAtEnd(string str) => isEOF() | isClosure() | this[Index].Equals(str);

        #endregion


        // Included for "Maybe it could be useful sometime"
        IOrderedEnumerable<string> IOrderedEnumerable<string>.CreateOrderedEnumerable<TKey>(Func<string, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            throw new NotImplementedException();
        }

        #region ErrorPrinter
        bool appendLog = true;
        const string logDir = "lwLogs";
        const string _logName = "log_listwalker";
        string logName = _logName; // To allow instanceIDs
        const string logSuffix = ".log";
        string logFile => logName + logSuffix;
        string logPath => $"{logDir}\\{logName}_{logCnt}{logSuffix}";
        int logCnt = 0;
        int lastError = 0;
        public static void deleteLogs() {
            try {
                if (System.IO.Directory.Exists(logDir) && instanceId == 1) {
                    System.IO.Directory.Delete(logDir, true);
                }
                Directory.GetFiles(".\\", _logName+".*").ToList().ForEach(s => File.Delete(s));
                //else System.IO.Directory.CreateDirectory(logDir);
            } catch (DirectoryNotFoundException) {} // Don't worry when dir doesn't exist
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error">What to print at lineEnd when the error is found. Default "ERROR" </param>
        /// <param name="finalize">(Default)False to return on the line after the error<para/>
        /// True to print everything since the last error (or the start) until the end</param>
        public void printError(string error = "ERROR", bool finalize = false)
        {
            string path; // $"logs\\log_{name}_{logCnt}.log";
            if ((appendLog && lastError == 0) || (appendLog == false && logCnt == 0)) {
                if (!appendLog) System.IO.Directory.CreateDirectory(logDir);
            }//*/
            ///<summary>Custom Index for ErrorWalking. Synced with <see cref="lastError"/> over multiple calls.</summary>
            int cnt = 0;
            /// Flag to trigger the Error at lineend
            bool foundError = false;
            /// Flag to mark stop of print when <paramref="finalize"/> is false.
            bool endPrint = false;
            path = (appendLog) ? logFile : logPath;
            var mode = (appendLog) ? System.IO.FileMode.Append : System.IO.FileMode.Create;
            using (var fs = new System.IO.FileStream(path, mode))
            using (var sw = new System.IO.StreamWriter(fs)) {
                if (finalize) sw.WriteLine("----END-OF-PARSE----");
                for (cnt = lastError; cnt < base.Count;) {
                    var token = this[cnt];
                    if (cnt == Index && !finalize) {
                        foundError = true;
                        sw.Write($" !!!{token}!!!");
                    } else sw.Write($" {token}");
                    if ("{};".Contains(token)) {
                        if (foundError) {
                            foundError = false;
                            sw.Write($"\t\t<--- {error}");
                            endPrint = true;
                        }
                        // For } ?
                        //fs.Seek(-(token.Length + 1), SeekOrigin.Current);
                        sw.WriteLine();
                    }
                    cnt++;
                    if (appendLog && endPrint) break;
                }
            }
            if (appendLog) lastError = cnt;
            else logCnt++;
        }
        #endregion

    }
    /// <summary>
    /// Generic version of ListWalker.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListWalker<T> : List<T>
    {
        public int Index { get; private set; }

        public T getCurrent() => this[Index];
        public T getNext() => this[++Index];
        /// <summary> Return "<see cref="this[Index]"/>.Equals(<paramref name="obj"/>)" </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool peekThis(T obj) => this[Index].Equals(obj);
        /// <summary> Return "<see cref="this[Index+1]"/>.Equals(<paramref name="obj"/>)" </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool peekNext(T obj) => this[Index + 1].Equals(obj);

        /// <summary>Increment index and checks if element != param.</summary>
        /// <param name="obj"></param>
        /// <returns>True if 'getNext()' != <paramref name="obj"/></returns>
        public bool nextNot(T obj) => !this.getNext().Equals(obj);

        /// <summary>
        /// Increment Index and return true when '<paramref name="obj"/> == this[<see cref="Index"/>]'.
        /// <para/>
        /// token = <see cref="getNext()"/>; return this[<see cref="Index"/>].Equals(<paramref name="obj"/>);
        /// </summary>
        /// <param name="token"></param>
        /// <param name="obj">The object to continue with.</param>
        /// <returns></returns>
        public bool whileTrue(ref T token, T obj) => (token = this.getNext()).Equals(obj);
        /// <summary>
        /// Increment Index and return true when '<paramref name="obj"/> != this[<see cref="Index"/>]'.
        /// <para/>
        /// token = <see cref="getNext()"/>; return !this[<see cref="Index"/>].Equals(<paramref name="obj"/>);
        /// </summary>
        /// <param name="token"></param>
        /// <param name="obj">The object to terminate with.</param>
        /// <returns></returns>
        public bool whileNot(ref T token, T obj) => !whileTrue(ref token, obj);
    }
}
