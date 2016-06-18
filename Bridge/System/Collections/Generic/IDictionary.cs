using Bridge;

namespace System.Collections.Generic
{
    [External]
    [Namespace("Bridge")]
    public interface IDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IBridgeClass
    {
        TValue this[TKey key]
        {
            get;
            set;
        }

        ICollection<TKey> Keys
        {
            get;
        }

        ICollection<TValue> Values
        {
            get;
        }

        int Count
        {
            get;
        }

        bool ContainsKey(TKey key);

        void Add(TKey key, TValue value);

        bool Remove(TKey key);

        bool TryGetValue(TKey key, out TValue value);
    }
}
