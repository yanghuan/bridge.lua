using Bridge;

namespace System.Collections.Generic
{
    [External]
    [Namespace("Bridge")]
    [Name("System.Dictionary")]
    public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, IBridgeClass
    {
        public Dictionary()
        {
        }

        public extern Dictionary(int capacity);

        public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
        }

        public Dictionary(object obj)
        {
        }

        public Dictionary(object obj, IEqualityComparer<TKey> comparer)
        {
        }

        public Dictionary(IEqualityComparer<TKey> comparer)
        {
        }

        public extern Dictionary(IDictionary<TKey, TValue> dictionary);

        public extern Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer);

        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return null;
            }
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return null;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return null;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return default(TValue);
            }
            set
            {
            }
        }

        public extern void Set(TKey key, TValue value);

        public extern void Add(TKey key, TValue value);

        public TValue Get(TKey key)
        {
            return default(TValue);
        }

        private TValue Items(TKey key)
        {
            return default(TValue);
        }

        public extern void Clear();

        public extern bool ContainsKey(TKey key);

        public extern bool ContainsValue(TValue value);

        public extern IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        extern IEnumerator IEnumerable.GetEnumerator();

        public extern bool Remove(TKey key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            return false;
        }
    }
}
