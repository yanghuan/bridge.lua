using Bridge;

namespace System.Collections
{
    [External]
    [Namespace("Bridge")]
    public interface ICollection : IEnumerable, IBridgeClass
    {
        int Count
        {
            get;
        }
    }
}
