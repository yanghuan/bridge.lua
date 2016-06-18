using Bridge;

namespace System.Collections.Generic
{
    [External]
    [Namespace("Bridge")]
    public interface IList<T> : ICollection<T>
    {
        T this[int index]
        {
            get;
            set;
        }

        int IndexOf(T item);

        void Insert(int index, T item);

        void RemoveAt(int index);
    }
}
