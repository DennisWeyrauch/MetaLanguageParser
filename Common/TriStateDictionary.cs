using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Common.Asserter;

//-->\r\n *\}\r\n *
//--> } 
namespace Common
{
    /// <summary>Represents a generic collection of key/value pairs.</summary>
    /// <typeparam name="TKey">The type of the key in the dictionary. Must be immutable.</typeparam>
    /// <typeparam name="TKey2">The type of the secondary key in the dictionary. Must be immutable.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    //[DefaultMember("Item")]
    public interface ITriDictionary<TKey, TKey2, TValue> : 
        /**/ICollection<KeyValueTrio<TKey, TKey2, TValue>>, IEnumerable<KeyValueTrio<TKey, TKey2, TValue>>, /*/
        ICollection<KeyValuePair<TKey, Dictionary<TKey2, TValue>>, IEnumerable<KeyValuePair<TKey, Dictionary<TKey2, TValue>>, 
        //*/ IEnumerable {

        /// <summary>Gets or sets the element with the specified key.</summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The element with the specified key.</returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and key is not found.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="ITriDictionary{K,K2,T}"/> is read-only.</exception>
        Dictionary<TKey2, TValue> this[TKey key] { get; set;  }

        /// <summary>Gets or sets the element with the specified key.</summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The element with the specified key.</returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and key is not found.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="ITriDictionary{K,K2,T}"/> is read-only.</exception>
        TValue this[TKey key, TKey2 key2] { get; set; }

        /// <summary>Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="ITriDictionary{K,K2,T}"/>.</summary>
        /// <returns>An <see cref="ICollection{T}"/> containing the keys of the object
        ///    that implements <see cref="ITriDictionary{K,K2,T}"/>.</returns>
        ICollection<KeyValuePair<TKey, TKey2>> Keys { get; }
        /// <summary>Gets an <see cref="ICollection{T}"/> containing the values in the
        ///    <see cref="ITriDictionary{K,K2,T}"/>.</summary>
        /// <returns>An <see cref="ICollection{T}"/> containing the values in the object
        ///    that implements <see cref="ITriDictionary{K,K2,T}"/>.</returns>
        ICollection<TValue> Values { get; } // From Dictionary

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        /// <summary>Adds an element with the provided key and value to the <see cref="ITriDictionary{K,K2,T}"/>.</summary>
        /// <param name="item">The <see cref="KeyValueTrio{TKey, TKey2, TValue}"/> to add.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="item"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key(s) already exists in the <see cref="ITriDictionary{K,K2,T}"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="ITriDictionary{K,K2,T}"/> is read-only.</exception>
        void Add(KeyValueTrio<TKey, TKey2, TValue> item);
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        /// <summary>Adds an element with the provided key and value to the <see cref="ITriDictionary{K,K2,T}"/>.</summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="ITriDictionary{K,K2,T}"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="ITriDictionary{K,K2,T}"/> is read-only.</exception>
        void Add(TKey key, TKey2 key2, TValue value);
        /// <summary>Determines whether the <see cref="ITriDictionary{K,K2,T}"/> contains an element
        ///    with the specified key.</summary>
        /// <param name="key">The key to locate in the <see cref="ITriDictionary{K,K2,T}"/>.</param>
        /// <returns>true if the <see cref="ITriDictionary{K,K2,T}"/> contains an element with
        ///    the key; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException">key are null.</exception>
        bool ContainsKey(TKey key); // From Dictionary
        /// <summary>Determines whether the <see cref="ITriDictionary{TKey, TKey2, TValue}"/> contains an element
        ///    with the specified keys</summary>
        /// <param name="key">The key to locate in the <see cref="ITriDictionary{TKey, TKey2, TValue}"/>.</param>
        /// <param name="key2">The second key to locate in the <see cref="ITriDictionary{TKey, TKey2, TValue}"/>.</param>
        /// <returns>true if the <see cref="ITriDictionary{TKey, TKey2, TValue}"/> contains an element with
        ///    the keys; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException">One or both keys are null.</exception>
        bool ContainsKey(TKey key, TKey2 key2);
        /// <summary>Removes the element with the specified key from the <see cref="ITriDictionary{K,K2,T}"/>.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false. This method also
        ///    returns false if key was not found in the original <see cref="ITriDictionary{K,K2,T}"/>.</returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="ITriDictionary{K,K2,T}"/> is read-only.</exception>
        bool Remove(TKey key);
        bool Remove(TKey key, TKey2 key2);
        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the
        ///    key is found; otherwise, the default value for the type of the value parameter.
        ///    This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements <see cref="ITriDictionary{K,K2,T}"/> contains
        ///    an element with the specified key; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        bool TryGetValue(TKey key, out Dictionary<TKey2, TValue> values);
        bool TryGetValue(TKey key, TKey2 key2, out TValue value);
    }

    // Note on documentation: http://stackoverflow.com/questions/759703/comment-the-interface-implementation-or-both
    /* Properties and other elements within an inherited class does not show the XML documentation in the tooltip
    when only specified on the interface. For external use of the same class, it is visible. This might be a bug with Visual Studio 2015
        //*/

    public class Dictionary<TKey, TKey2, TValue> : Dictionary<TKey, Dictionary<TKey2, TValue>>, ITriDictionary<TKey, TKey2, TValue>
    {
        #region Implemented members

        public new Dictionary<TKey2, TValue> this[TKey key]
        {
            get{ throw new NotImplementedException(); }
            set{ throw new NotImplementedException(); }
        }
        public TValue this[TKey key, TKey2 key2]
        {
            get{ throw new NotImplementedException(); }
            set{ throw new NotImplementedException(); }
        }
        
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public int Count{ get { throw new NotImplementedException(); } }
        public bool IsReadOnly => false;//{ get { throw new NotImplementedException(); } }//*/    Not Part of the Pragma
        public ICollection<KeyValuePair<TKey, TKey2>> Keys { get { throw new NotImplementedException(); } }
        public ICollection<TValue> Values { get { throw new NotImplementedException(); } }
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        #endregion
        // Add stuff
        public void Add(KeyValueTrio<TKey, TKey2, TValue> item) => Add(item.Key1, item.Key2, item.Value);
        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="key2"></param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <exception cref="ArgumentNullException">Either of the keys is null.</exception>
        /// <exception cref="ArgumentException">An element with the same keys already exists in the <see cref="Dictionary{TKey, TKey2, TValue}"/></exception>
        public void Add(TKey key, TKey2 key2, TValue value) {
            AssertNotNull(key, nameof(key));
            AssertNotNull(key2, nameof(key2));
            Dictionary<TKey2, TValue> innerDict;
            if (!base.TryGetValue(key, out innerDict)) {
                //innerDict = new Dictionary<TKey2, TValue>();  innerDict.Add(key2, value); // Smaller, but...
                innerDict = new Dictionary<TKey2, TValue>() { { key2, value } };
                this.Add(key, innerDict);
                return;
            }
            if (!innerDict.ContainsKey(key2)) {
                innerDict.Add(key2, value);
            } else {
                throw new ArgumentException($"KeyPair \"{key}-{key2}\" already exists.");
            }
        }
        
        //public void Clear() => this.Clear();
        public bool Contains__(KeyValueTrio<TKey, TKey2, TValue> item){
            //return this[item.Key1].Contains(new KeyValuePair<TKey2, TValue>(item.Key2, item.Value));
            AssertNotNull(item, nameof(item));
            if (this.ContainsKey(item.Key1)) {
                var kv = new KeyValuePair<TKey2, TValue>(item.Key2, item.Value);
                return this[item.Key1].Contains(kv);
            }
            return false;
        }
        public bool Contains(KeyValueTrio<TKey, TKey2, TValue> item)
            => (bool)this?[item.Key1]?.Contains(new KeyValuePair<TKey2, TValue>(item.Key2, item.Value));
        /* Nullable Check: 
        call Elem with '?', dup, brtrue


            //*/

        //public bool ContainsKey(TKey key) => this.ContainsKey(key);
        public bool ContainsKey(TKey key, TKey2 key2){ throw new NotImplementedException(); }
        public void CopyTo(KeyValueTrio<TKey, TKey2, TValue>[] array, int arrayIndex){ throw new NotImplementedException(); }
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public IEnumerator<KeyValueTrio<TKey, TKey2, TValue>> GetEnumerator(){ throw new NotImplementedException(); }
        public bool Remove(KeyValueTrio<TKey, TKey2, TValue> item){ throw new NotImplementedException(); } // NOT Part of the Pragma
        public bool Remove(TKey key){ throw new NotImplementedException(); }
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        public bool Remove(TKey key, TKey2 key2){ throw new NotImplementedException(); }

        //public bool TryGetValue(TKey key, out Dictionary<TKey2, TValue> values) => base.TryGetValue(key, out values);
        /// <summary>Gets the value associated with the specified keys.</summary>
        /// <param name="key">The master key of the value to get.</param>
        /// <param name="key2">The inner key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified keys
        /// if the keys are found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if the <see cref="Dictionary{TKey, TKey2, TValue}"/> contains an element with 
        /// the specified keys; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Either of the keys is null.</exception>
        public bool TryGetValue(TKey key, TKey2 key2, out TValue value){
            AssertNotNull(key, nameof(key));
            AssertNotNull(key2, nameof(key2));
            Dictionary<TKey2, TValue> innerDict;
            if (base.TryGetValue(key, out innerDict)){
                if(innerDict.TryGetValue(key2, out value)) {
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }
        IEnumerator IEnumerable.GetEnumerator(){ throw new NotImplementedException(); }
        /*
        public void Add(KeyKeyValueTrio<TKey, TKey2, TValue> item){ throw new NotImplementedException(); }
        public void Add(TKey key, TKey2 key2, TValue value){ throw new NotImplementedException(); }
        public void Clear(){ throw new NotImplementedException(); }
        public bool Contains(KeyKeyValueTrio<TKey, TKey2, TValue> item){ throw new NotImplementedException(); }
        public bool ContainsKey(TKey key){ throw new NotImplementedException(); }
        public bool ContainsKey(TKey key, TKey2 key2){ throw new NotImplementedException(); }
        public void CopyTo(KeyKeyValueTrio<TKey, TKey2, TValue>[] array, int arrayIndex){ throw new NotImplementedException(); }
        public IEnumerator<KeyKeyValueTrio<TKey, TKey2, TValue>> GetEnumerator(){ throw new NotImplementedException(); }
        public bool Remove(KeyKeyValueTrio<TKey, TKey2, TValue> item){ throw new NotImplementedException(); }
        public bool Remove(TKey key){ throw new NotImplementedException(); }
        public bool Remove(TKey key, TKey2 key2){ throw new NotImplementedException(); }
        public bool TryGetValue(TKey key, out Dictionary<TKey2, TValue> values){ throw new NotImplementedException(); }
        public bool TryGetValue(TKey key, TKey2 key2, out TValue value){ throw new NotImplementedException(); }
        IEnumerator IEnumerable.GetEnumerator(){ throw new NotImplementedException(); }
        //#endregion
        //*/
    }
    public class KeyValueTrio<TKey, TKey2, TValue>
    {
        public TKey Key1;
        public TKey2 Key2;
        public TValue Value;
    }


}
