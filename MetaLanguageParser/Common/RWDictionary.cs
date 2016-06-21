using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Common
{
    /// <summary>
    /// Represents an ordered dictionary with read/write access and non-muteable KeyValuePairs.<para/>
    /// This is realized with a <see cref="List{T}"/> (T being <see cref="RWDictElement{TKey, TValue}"/>) that implements 
    /// <see cref="IDictionary{TKey, TValue}"/> to provide the order of a list with the lookup of a Dictionary.
    /// It provides fluent transition between <see cref="KeyValuePair{TKey, TValue}"/> and <see cref="RWDictElement{TKey, TValue}"/>. 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class RWDictionary<TKey, TValue> : List<RWDictElement<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        #region Fields and Indexers
        /// <summary>Gets or sets the element with the specified key.</summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The element with the specified key. See <see cref="List{T}.this[int]"/></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found</exception>
        public TValue this[TKey key]
        {
            get { return base[FindIndex(key)].Value; }
            set { base[FindIndex(key)].Value = value; }
        }

        /// <summary>Gets the Value of the element at position <paramref name="idx"/></summary>
        /// <param name="idx">Index to lookup</param>
        /// <returns>See <see cref="List{T}.this[int]"/></returns>
        public new TValue this[int idx]
        { // "new" so that locally base[idx] can still be used, but is replaced with this outwards
            get { return base[idx].Value; }
            set { base[idx].Value = value; }
        }

        /// <summary>Gets an System.Collections.Generic.ICollection`1 containing the keys of the System.Collections.Generic.IDictionary`2.</summary>
        /// <returns>An System.Collections.Generic.ICollection`1 containing the keys of the object\r\n that implements System.Collections.Generic.IDictionary`2.</returns>
        public ICollection<TKey> Keys
        {
            get
            {
                ICollection<TKey> keys = new List<TKey>();
                foreach (var item in this) {
                    keys.Add(item.Key);
                }
                return keys;
            }
        }

        /// <summary>Gets an System.Collections.Generic.ICollection`1 containing the values in the\r\n 
        /// System.Collections.Generic.IDictionary`2.</summary>
        /// <returns>An System.Collections.Generic.ICollection`1 containing the values in the object\r\n 
        /// that implements System.Collections.Generic.IDictionary`2.</returns>
        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> values = new List<TValue>();
                foreach (var item in this) {
                    values.Add(item.Value);
                }
                return values;
            }
        }
        //public TValue this[TKey key] {...}
        public bool IsReadOnly => false;


        /// <summary>Retrieves the key at position <paramref name="idx"/></summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public TKey getKey(int idx) => base[idx].Key;
        public RWDictElement<TKey, TValue> getElem(TKey k) => base[FindIndex(k)];

        //List<RWDictElement<TKey, TValue>>.FindIndex(Predicate<RWDictElement<TKey, TValue>> match)
        /// <summary>
        /// See <see cref="List{T}.FindIndex(Predicate{T} match)"/><para/>
        /// Calls "List{T}.FindIndex() and searches for the first entry whoose Key matches <paramref name="k"/>
        /// </summary>
        /// <param name="k"></param>
        /// <returns>Zero-based index of first match found, if any; otherwise, -1</returns>
        public int FindIndex(TKey k)
        {
            return base.FindIndex((RWDictElement<TKey, TValue> da) => {
                return da.Key.Equals(k);
            });
        }


        #endregion

        #region IDictionary-Methods
        /// <summary>Adds an element with the provided key and value to the System.Collections.Generic.IDictionary`2.</summary>
        /// <param name="k">The object to use as the key of the element to add.</param>
        /// <param name="v">The object to use as the value of the element to add.</param>
        /// <param name="overwrite">Whether or not to force assignment if <paramref name="k"/> already exists</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="k"/> is null.</exception>
        /// <exception cref="System.ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary`2 and <paramref name="overwrite"/> is false.</exception>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.IDictionary`2 is read-only.</exception>
        public void Add(TKey k, TValue v, bool overwrite)
        {
            if (k == null) throw new ArgumentNullException("Key is null");
            if (FindIndex(k) != -1) {
                if (overwrite) {
                    base[FindIndex(k)].Value = v;
                } else throw new System.ArgumentException($"Key '{k}' already exists");
            } else base.Add(new RWDictElement<TKey, TValue>(k, v));
        }

        /// <summary>Adds an element with the provided key and value to the System.Collections.Generic.IDictionary`2.</summary>
        /// <param name="k">The object to use as the key of the element to add.</param>
        /// <param name="v">The object to use as the value of the element to add.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="k"/> is null.</exception>
        /// <exception cref="System.ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary`2.</exception>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.IDictionary`2 is read-only.</exception>
        public void Add(TKey k, TValue v) => this.Add(k, v, false);

        /// <summary>Adds an element with the provided key and value to the System.Collections.Generic.IDictionary`2.</summary>
        /// <param name="k">The object to use as the key of the element to replace.</param>
        /// <param name="v">The object to use as the new value of the element.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="k"/> is null.</exception>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.IDictionary`2 is read-only.</exception>
        public void Replace(TKey k, TValue v) => this.Add(k, v, true);
        /*
    }

    // This partial part is to make the type fully compatible with the IDictionary Interface (from which all descriptions are copied over)
    public partial class RWDictionary<TKey, TValue> : List<RWDictElement<TKey, TValue>>, IDictionary<TKey, TValue>
    {//*/
        //#region Fields and Indexers
        //#endregion
        //#region IDictionary-Methods

        /// <summary>Determines whether the System.Collections.Generic.IDictionary`2 contains an element\r\n with the specified key.</summary>
        /// <param name="key">The key to locate in the System.Collections.Generic.IDictionary`2.</param>
        /// <returns>true if the System.Collections.Generic.IDictionary`2 contains an element with\r\n the key; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key) => (FindIndex(key) != -1);
        /// <summary>Removes the element with the specified key from the System.Collections.Generic.IDictionary`2.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false. This method also\r\n returns false if <paramref name="key"/> was not found in the original System.Collections.Generic.IDictionary`2.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.IDictionary`2 is read-only.</exception>
        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException("Key is null");
            try {
                base.RemoveAt(FindIndex(key));
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the\r\n key is found; otherwise, the default value for the type of the value parameter.\r\n This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements System.Collections.Generic.IDictionary`2 contains\r\n an element with the specified key; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException("Key is null");
            int idx = FindIndex(key);
            if (idx != -1) {
                value = base[idx].Value;
                return true;
            } else {
                value = default(TValue);
                return false;
            }
        }
        #endregion
        #region ICollection-Methods

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public bool Contains(KeyValuePair<TKey, TValue> item) => base.Contains(item);

#warning ICollection<T>.CopyTo(KeyValuePair<>, int) is not tested
        // Required override of IDictionary (Explicit since List already defines it via IList<-ICollection)
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            base.CopyTo(RWDictElement<TKey, TValue>.Convert(array), arrayIndex);
        }
        // Public accessor of above
        //public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<TKey, TValue> item) => base.Remove(item);//Remove(item.Key);

#warning IEnumerable.GetEnumerator() is not tested
        ReadOnlyDictionary<TKey, TValue> rodict;
        /// <summary>
        /// Converts the dictionary into a <see cref="System.Collections.ObjectModel.ReadOnlyDictionary{TKey, TValue}"/> and returns its Enumerator.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            rodict = new ReadOnlyDictionary<TKey, TValue>(this);
            return rodict.GetEnumerator();
        }
        #endregion

        /// <returns>String representation of this object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var elem in this) {
                sb.AppendLine(elem.ToString());
            }
            return sb.ToString();
        }

    }

    /// <summary>
    /// <see cref="KeyValuePair{TKey, TValue}"/> with additional setter. Provides fluent transition between both.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class RWDictElement<TKey, TValue>
    {

        private TKey _key;
        public TKey Key => _key;
        public TValue Value;

        private RWDictElement() { }
        public RWDictElement(TKey k, TValue v)
        {
            this._key = k;
            this.Value = v;
        }

        #region RWDictElement to KeyValuePair
        public static implicit operator KeyValuePair<TKey, TValue>(RWDictElement<TKey, TValue> rw)
            => new KeyValuePair<TKey, TValue>(rw.Key, rw.Value);

        public static KeyValuePair<TKey, TValue>[] Convert(RWDictElement<TKey, TValue>[] rw)
            => Array.ConvertAll(rw, (item) => (KeyValuePair<TKey, TValue>)item);

        #endregion

        #region KeyValuePair to RWDictElement
        public RWDictElement(KeyValuePair<TKey, TValue> kw)
        {
            this._key = kw.Key;
            this.Value = kw.Value;
        }

        public static implicit operator RWDictElement<TKey, TValue>(KeyValuePair<TKey, TValue> kv)
            => new RWDictElement<TKey, TValue>(kv.Key, kv.Value);

        public static RWDictElement<TKey, TValue>[] Convert(KeyValuePair<TKey, TValue>[] kv)
            => Array.ConvertAll(kv, (item) => (RWDictElement<TKey, TValue>)item);

        #endregion

#warning RWDictElement.Equals(object) is not tested
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var rw = obj as RWDictElement<TKey, TValue>;
            return Key.Equals(rw.Key) && Value.Equals(rw.Value);
        }
        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{Key.ToString()} : {Value.ToString()}";
    }

}
