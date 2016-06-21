#if false
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//-->\r\n *\}\r\n *
//--> } 
namespace Common
{
    public class AssertDictionary<TKey, TAssert, TValue> : IDictionary<TKey, TValue>
    {
        #region Implemented members
        public TValue this[TKey key] {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Count { get { throw new NotImplementedException(); } }
        public bool IsReadOnly { get { throw new NotImplementedException(); } }

        public ICollection<TKey> Keys { get { throw new NotImplementedException(); } }
        public ICollection<TValue> Values { get { throw new NotImplementedException(); } }

        public void Add(KeyValuePair<TKey, TValue> item) { throw new NotImplementedException(); } 
        public void Add(TKey key, TValue value) { throw new NotImplementedException(); } 
        public void Clear() { throw new NotImplementedException(); } 
        public bool Contains(KeyValuePair<TKey, TValue> item) { throw new NotImplementedException(); } 
        public bool ContainsKey(TKey key) { throw new NotImplementedException(); } 
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { throw new NotImplementedException(); } 
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { throw new NotImplementedException(); } 
        public bool Remove(KeyValuePair<TKey, TValue> item) { throw new NotImplementedException(); } 
        public bool Remove(TKey key) { throw new NotImplementedException(); } 
        public bool TryGetValue(TKey key, out TValue value) { throw new NotImplementedException(); } 
        IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
        #endregion
#warning Two raw Exceptions
        public bool setIsValue(TKey key, TAssert isVal)
        {
            // Get HelperType
            AssertValue av = new AssertValue();
            
            if (av.mustValue == null) {
                if (av.isValue == null) { av.isValue = isVal; return true; }
                if (!av.matchIs(isVal)) {
                    // First try to look if an implicit cast for the two exists. ?? (Use? Only checking for access here)
                    throw new Exception("References do not match"); // ??
                }
            } else if (!av.match(isVal)) throw new Exception("References do not match");
            return false;
        }

        class AssertValue
        {
            internal TAssert isValue;
            internal TAssert mustValue;
            internal TValue value;

            internal bool matchIs(TAssert isVal) => isValue.Equals(isVal);
            internal bool match(TAssert isVal) => mustValue.Equals(isVal);

        }
    }
}
#endif