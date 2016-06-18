using Bridge;

namespace System.Collections.Generic
{
    [External]
    [Namespace("Bridge")]
    public interface ICollection<T> : IEnumerable<T>, IBridgeClass
    {
        int Count
        {
            [Template("#{this}")]
            get;
        }

        void Add(T item);

        void Clear();

        bool Contains(T item);

        bool Remove(T item);
    }
}
