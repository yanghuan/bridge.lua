using Bridge;
using System.Collections.Generic;

namespace System.Linq
{
    [External]
    [IgnoreGeneric]
    public interface IGrouping<out TKey, TElement> : IEnumerable<TElement>
    {
        [FieldProperty]
        TKey Key
        {
            get;
        }
    }

    [External]
    [IgnoreGeneric]
    public class Grouping<TKey, TElement> : EnumerableInstance<TElement>, IGrouping<TKey, TElement>
    {
        internal Grouping()
        {
        }

        [FieldProperty]
        public TKey Key
        {
            get
            {
                return default(TKey);
            }
        }
    }
}
