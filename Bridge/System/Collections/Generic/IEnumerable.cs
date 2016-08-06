using Bridge;

namespace System.Collections.Generic
{
    [External]
    [Namespace("Bridge")]
    public interface IEnumerable<out T> : IEnumerable, IBridgeClass
    {
        new IEnumerator<T> GetEnumerator();
    }
}
