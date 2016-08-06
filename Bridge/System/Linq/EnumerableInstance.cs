using Bridge;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    [External]
    [IgnoreGeneric]
    public class EnumerableInstance<TElement> : IEnumerable<TElement> {
        internal EnumerableInstance() { }

        public extern IEnumerator<TElement> GetEnumerator();

        extern IEnumerator IEnumerable.GetEnumerator();

        public TElement Aggregate(Func<TElement, TElement, TElement> func) {
            return default(TElement);
        }

        public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, Func<TAccumulate, TElement, TAccumulate> func) {
            return default(TAccumulate);
        }

        public TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, Func<TAccumulate, TElement, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector) {
            return default(TResult);
        }

        public extern bool All(Func<TElement, bool> predicate);

        public extern EnumerableInstance<TElement> Alternate(TElement value);

        public extern bool Any();

        public extern bool Any(Func<TElement, bool> predicate);

        public extern double Average(Func<TElement, int> selector);

        public extern double Average(Func<TElement, long> selector);

        public extern float Average(Func<TElement, float> selector);

        public extern double Average(Func<TElement, double> selector);

        public extern decimal Average(Func<TElement, decimal> selector);

        public extern EnumerableInstance<TElement[]> Buffer(int count);

        public extern EnumerableInstance<TElement> CascadeBreadthFirst(Func<TElement, IEnumerable<TElement>> func);

        public extern EnumerableInstance<TResult> CascadeBreadthFirst<TResult>(Func<TElement, IEnumerable<TElement>> func,
            Func<TElement, TResult> resultSelector);

        public extern EnumerableInstance<TResult> CascadeBreadthFirst<TResult>(Func<TElement, IEnumerable<TElement>> func,
            Func<TElement, int, TResult> resultSelector);

        public extern EnumerableInstance<TElement> CascadeDepthFirst(Func<TElement, IEnumerable<TElement>> func);

        public extern EnumerableInstance<TResult> CascadeDepthFirst<TResult>(Func<TElement, IEnumerable<TElement>> func,
            Func<TElement, TResult> resultSelector);

        public extern EnumerableInstance<TResult> CascadeDepthFirst<TResult>(Func<TElement, IEnumerable<TElement>> func,
            Func<TElement, int, TResult> resultSelector);

        public extern EnumerableInstance<TResult> Cast<TResult>();

        public extern EnumerableInstance<TElement> CatchError(Action<Exception> action);

        public extern EnumerableInstance<TElement> Concat(IEnumerable<TElement> other);

        public extern bool Contains(TElement value);

        public extern bool Contains(TElement value, IEqualityComparer<TElement> comparer);

        public extern int Count();

        public extern int Count(Func<TElement, bool> predicate);

        public extern EnumerableInstance<TElement> DefaultIfEmpty();

        public extern EnumerableInstance<TElement> DefaultIfEmpty(TElement defaultValue);

        public extern EnumerableInstance<TElement> Distinct();

        public extern EnumerableInstance<TElement> Distinct(IEqualityComparer<TElement> comparer);

        public extern EnumerableInstance<TElement> DoAction(Action<TElement> action);

        public extern EnumerableInstance<TElement> DoAction(Action<TElement, int> action);

        public TElement ElementAt(int index) {
            return default(TElement);
        }

        public TElement ElementAtOrDefault(int index) {
            return default(TElement);
        }

        public TElement ElementAtOrDefault(int index, TElement defaultValue) {
            return default(TElement);
        }

        public extern EnumerableInstance<TElement> Except(IEnumerable<TElement> other);

        public extern EnumerableInstance<TElement> Except(IEnumerable<TElement> other, IEqualityComparer<TElement> comparer);

        public extern EnumerableInstance<TElement> FinallyAction(Action action);

        public TElement First() {
            return default(TElement);
        }

        public TElement First(Func<TElement, bool> predicate) {
            return default(TElement);
        }

        public TElement FirstOrDefault() {
            return default(TElement);
        }

        public TElement FirstOrDefault(TElement defaultValue) {
            return default(TElement);
        }

        public TElement FirstOrDefault(Func<TElement, bool> predicate) {
            return default(TElement);
        }

        public TElement FirstOrDefault(Func<TElement, bool> predicate, TElement defaultValue) {
            return default(TElement);
        }

        public extern EnumerableInstance<object> Flatten();

        public extern void Force();

        public extern void ForEach(Action<TElement> action);

        public extern void ForEach(Func<TElement, bool> action);

        public extern void ForEach(Action<TElement, int> action);

        public extern void ForEach(Func<TElement, int, bool> action);

        [Template("{this}:groupBy({keySelector}, {TKey})")]
        public extern EnumerableInstance<Grouping<TKey, TElement>> GroupBy<TKey>(Func<TElement, TKey> keySelector);

        [Template("{this}:groupBy({keySelector}, {comparer}, {TKey})")]
        public extern EnumerableInstance<Grouping<TKey, TElement>> GroupBy<TKey>(Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer);

        [Template("{this}:groupBy({keySelector}, {elementSelector}, {TKey}, {TSource})")]
        public extern EnumerableInstance<Grouping<TKey, TSource>> GroupBy<TKey, TSource>(Func<TElement, TKey> keySelector, Func<TElement, TSource> elementSelector);

        [Template("{this}:groupBySelect({keySelector}, {resultSelector}, {TResult})")]
        public extern EnumerableInstance<TResult> GroupBy<TKey, TResult>(Func<TElement, TKey> keySelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector);

        [Template("{this}:groupBy({keySelector}, {elementSelector}, {comparer}, {TKey}, {TSource})")]
        public extern EnumerableInstance<Grouping<TKey, TSource>> GroupBy<TKey, TSource>(Func<TSource, TKey> keySelector,
            Func<TSource, TSource> elementSelector, IEqualityComparer<TKey> comparer);

        [Template("{this}:groupBySelect({keySelector}, {elementSelector}, {resultSelector}, {TKey}, {TSource}, {TResult})")]
        public extern EnumerableInstance<TResult> GroupBy<TKey, TSource, TResult>(Func<TSource, TKey> keySelector,
            Func<TSource, TSource> elementSelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector);

        [Template("{this}:groupBySelect({keySelector}, {resultSelector}, {comparer}, {TKey}, {TResult})")]
        public extern EnumerableInstance<TResult> GroupBy<TKey, TResult>(Func<TElement, TKey> keySelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer);

        [Template("{this}:groupBySelect({keySelector}, {elementSelector}, {resultSelector}, {comperer}, {TKey}, {TSource}, {resultSelector})")]
        public extern EnumerableInstance<TResult> GroupBy<TKey, TSource, TResult>(Func<TSource, TKey> keySelector,
            Func<TSource, TSource> elementSelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey> comperer);

        public extern EnumerableInstance<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner,
            Func<TElement, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TElement, IEnumerable<TInner>, TResult> resultSelector);

        public extern EnumerableInstance<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner,
            Func<TElement, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TElement, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer);

        public extern int IndexOf(TElement item);

        public extern int IndexOf(TElement item, Func<TElement, bool> predicate);

        public extern int IndexOf(TElement item, IEqualityComparer<TElement> comparer);

        public extern EnumerableInstance<TElement> Insert(int index, IEnumerable<TElement> other);

        public extern EnumerableInstance<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner,
            Func<TElement, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TElement, TInner, TResult> resultSelector);

        public extern EnumerableInstance<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner,
            Func<TElement, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TElement, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer);

        public TElement Last()
        {
            return default(TElement);
        }

        public TElement Last(Func<TElement, bool> predicate)
        {
            return default(TElement);
        }

        public extern int LastIndexOf(TElement item);

        public extern int LastIndexOf(TElement item, Func<TElement, bool> predicate);

        public extern int LastIndexOf(TElement item, IEqualityComparer<TElement> comparer);

        [Template("{this}:count()")]
        public static long LongCount<TSource>()
        {
            return default(long);
        }

        [Template("{this}:count({predicate})")]
        public static long LongCount<TSource>(Func<TSource, bool> predicate)
        {
            return default(long);
        }

        public TElement LastOrDefault()
        {
            return default(TElement);
        }

        public TElement LastOrDefault(TElement defaultValue)
        {
            return default(TElement);
        }

        public TElement LastOrDefault(Func<TElement, bool> predicate)
        {
            return default(TElement);
        }

        public TElement LastOrDefault(Func<TElement, bool> predicate, TElement defaultValue)
        {
            return default(TElement);
        }

        public extern EnumerableInstance<TResult> LetBind<TResult>(Func<IEnumerable<TElement>, IEnumerable<TResult>> func);

        public static TSource Max<TSource>()
        {
            return default(TSource);
        }

        public static TResult Max<TSource, TResult>(Func<TSource, TResult> selector)
        {
            return default(TResult);
        }

        public extern int Max(Func<TElement, int> selector);

        public extern long Max(Func<TElement, long> selector);

        public extern float Max(Func<TElement, float> selector);

        public extern double Max(Func<TElement, double> selector);

        public extern decimal Max(Func<TElement, decimal> selector);

        public TElement MaxBy(Func<TElement, int> selector)
        {
            return default(TElement);
        }

        public TElement MaxBy(Func<TElement, long> selector)
        {
            return default(TElement);
        }

        public TElement MaxBy(Func<TElement, float> selector)
        {
            return default(TElement);
        }

        public TElement MaxBy(Func<TElement, double> selector)
        {
            return default(TElement);
        }

        public TElement MaxBy(Func<TElement, decimal> selector)
        {
            return default(TElement);
        }

        public extern EnumerableInstance<TElement> Memoize();

        public static TSource Min<TSource>()
        {
            return default(TSource);
        }

        public static TResult Min<TSource, TResult>(Func<TSource, TResult> selector)
        {
            return default(TResult);
        }

        public extern int Min(Func<TElement, int> selector);

        public extern long Min(Func<TElement, long> selector);

        public extern float Min(Func<TElement, float> selector);

        public extern double Min(Func<TElement, double> selector);

        public extern decimal Min(Func<TElement, decimal> selector);

        public TElement MinBy(Func<TElement, int> selector)
        {
            return default(TElement);
        }

        public TElement MinBy(Func<TElement, long> selector)
        {
            return default(TElement);
        }

        public TElement MinBy(Func<TElement, float> selector)
        {
            return default(TElement);
        }

        public TElement MinBy(Func<TElement, double> selector)
        {
            return default(TElement);
        }

        public TElement MinBy(Func<TElement, decimal> selector)
        {
            return default(TElement);
        }

        [Template("{this}:ofType({TResult})")]
        public extern EnumerableInstance<TResult> OfType<TResult>();

        public extern OrderedEnumerable<TElement> OrderBy();

        public extern OrderedEnumerable<TElement> OrderBy<TKey>(Func<TElement, TKey> keySelector);

        public extern OrderedEnumerable<TElement> OrderBy<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer);

        public extern OrderedEnumerable<TElement> OrderByDescending();

        public extern OrderedEnumerable<TElement> OrderByDescending<TKey>(Func<TElement, TKey> keySelector);

        public extern OrderedEnumerable<TElement> OrderByDescending<TKey>(Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer);

        public extern EnumerableInstance<TResult> Pairwise<TResult>(Func<TElement, TElement, TResult> selector);

        public extern EnumerableInstance<Grouping<TKey, TElement>> PartitionBy<TKey>(Func<TElement, TKey> keySelector);

        [Template("{this}:partitionBy({keySelector}, null, null, {comparer})")]
        public extern EnumerableInstance<Grouping<TKey, TElement>> PartitionBy<TKey>(Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey> comparer);

        public extern EnumerableInstance<Grouping<TKey, TSource>> PartitionBy<TKey, TSource>(
            Func<TSource, TKey> keySelector, Func<TSource, TSource> elementSelector);

        [Template("{this}:partitionBy({keySelector}, null, {resultSelector})")]
        public extern EnumerableInstance<TResult> PartitionBy<TKey, TResult>(Func<TElement, TKey> keySelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector);

        [Template("{this}:partitionBy({keySelector}, {elementSelector}, null, {comparer})")]
        public extern EnumerableInstance<Grouping<TKey, TSource>> PartitionBy<TKey, TSource>(
            Func<TSource, TKey> keySelector, Func<TSource, TSource> elementSelector, IEqualityComparer<TKey> comparer);

        public extern EnumerableInstance<TResult> PartitionBy<TKey, TSource, TResult>(Func<TSource, TKey> keySelector,
            Func<TSource, TSource> elementSelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector);

        [Template("{this}:partitionBy({keySelector}, null, {resultSelector}, {comparer})")]
        public extern EnumerableInstance<TResult> PartitionBy<TKey, TResult>(Func<TElement, TKey> keySelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer);

        public extern EnumerableInstance<TResult> PartitionBy<TKey, TSource, TResult>(Func<TSource, TKey> keySelector,
            Func<TSource, TSource> elementSelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey> comperer);

        public extern EnumerableInstance<TElement> Reverse();

        public static extern EnumerableInstance<T> Scan<T>(Func<T, T, T> func);

        public extern EnumerableInstance<TAccumulate> Scan<TAccumulate>(TAccumulate seed,
            Func<TAccumulate, TElement, TAccumulate> func);

        [Template("{this}:select({selector}, {TResult})")]
        public extern EnumerableInstance<TResult> Select<TResult>(Func<TElement, TResult> selector);

        [Template("{this}:select({selector}, {TResult})")]
        public extern EnumerableInstance<TResult> Select<TResult>(Func<TElement, int, TResult> selector);

        [Template("{this}:selectMany({selector}, {TResult})")]
        public extern EnumerableInstance<TResult> SelectMany<TResult>(Func<TElement, IEnumerable<TResult>> selector);

        [Template("{this}:selectMany({selector}, {TResult})")]
        public extern EnumerableInstance<TResult> SelectMany<TResult>(Func<TElement, int, IEnumerable<TResult>> selector);

        [Template("{this}:selectMany({collectionSelector}, {resultSelector}, {TResult})")]
        public extern EnumerableInstance<TResult> SelectMany<TCollection, TResult>(
            Func<TElement, IEnumerable<TCollection>> collectionSelector,
            Func<TElement, TCollection, TResult> resultSelector);

        [Template("{this}:selectMany({collectionSelector}, {resultSelector}, {TResult})")]
        public extern EnumerableInstance<TResult> SelectMany<TCollection, TResult>(
            Func<TElement, int, IEnumerable<TCollection>> collectionSelector,
            Func<TElement, TCollection, TResult> resultSelector);

        public extern bool SequenceEqual(IEnumerable<TElement> other);

        public extern bool SequenceEqual<TKey>(IEnumerable<TElement> other, Func<TElement, TKey> compareSelector);

        public extern EnumerableInstance<TElement> Share();

        public extern EnumerableInstance<TElement> Shuffle();

        public TElement Single()
        {
            return default(TElement);
        }

        public TElement Single(Func<TElement, bool> predicate)
        {
            return default(TElement);
        }

        public TElement SingleOrDefault()
        {
            return default(TElement);
        }

        public TElement SingleOrDefault(TElement defaultValue)
        {
            return default(TElement);
        }

        public TElement SingleOrDefault(Func<TElement, bool> predicate)
        {
            return default(TElement);
        }

        public TElement SingleOrDefault(Func<TElement, bool> predicate, TElement defaultValue)
        {
            return default(TElement);
        }

        public extern EnumerableInstance<TElement> Skip(int count);

        public extern EnumerableInstance<TElement> SkipWhile(Func<TElement, bool> predicate);

        public extern EnumerableInstance<TElement> SkipWhile(Func<TElement, int, bool> predicate);

        public extern int Sum(Func<TElement, int> selector);

        public extern long Sum(Func<TElement, long> selector);

        public extern float Sum(Func<TElement, float> selector);

        public extern double Sum(Func<TElement, double> selector);

        public extern decimal Sum(Func<TElement, decimal> selector);

        public extern EnumerableInstance<TElement> Take(int count);

        public extern EnumerableInstance<TElement> TakeExceptLast();

        public extern EnumerableInstance<TElement> TakeExceptLast(int count);

        public extern EnumerableInstance<TElement> TakeFromLast(int count);

        public extern EnumerableInstance<TElement> TakeWhile(Func<TElement, bool> predicate);

        public extern EnumerableInstance<TElement> TakeWhile(Func<TElement, int, bool> predicate);

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(Func<TSource, TKey> keySelector)
        {
            return default(IOrderedEnumerable<TSource>);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            return default(IOrderedEnumerable<TSource>);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(Func<TSource, TKey> keySelector)
        {
            return default(IOrderedEnumerable<TSource>);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            return default(IOrderedEnumerable<TSource>);
        }

        public extern TElement[] ToArray();

        [Template("{this}:toDictionary({keySelector}, {TKey})")]
        public extern Dictionary<TKey, TElement> ToDictionary<TKey>(Func<TElement, TKey> keySelector);

        [Template("{this}:toDictionary({keySelector}, {comparer}, {TKey})")]
        public extern Dictionary<TKey, TElement> ToDictionary<TKey>(Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey> comparer);

        [Template("{this}:toDictionary({keySelector}, {elementSelector}, {TKey}, {TValue})")]
        public extern Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<TElement, TKey> keySelector,
            Func<TElement, TValue> elementSelector);

        [Template("{this}:toDictionary({keySelector}, {elementSelector}, {comparer}, {TKey}, {TValue})")]
        public extern Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<TElement, TKey> keySelector,
            Func<TElement, TValue> elementSelector, IEqualityComparer<TKey> comparer);

        public extern List<TElement> ToList();

        [Template("{this}:toLookup({keySelector}, {TKey})")]
        public extern Lookup<TKey, TElement> ToLookup<TKey>(Func<TElement, TKey> keySelector);

        [Template("{this}:toLookup({keySelector}, {comparer}, {TKey})")]
        public extern Lookup<TKey, TElement> ToLookup<TKey>(Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer);

        [Template("{this}:toLookup({keySelector}, {elementSelector}, {TKey}, {TSource})")]
        public extern Lookup<TKey, TSource> ToLookup<TKey, TSource>(Func<TElement, TKey> keySelector,
            Func<TElement, TSource> elementSelector);

        [Template("{this}:toLookup({keySelector}, {elementSelector}, {comparer}, {TKey}, {TSource})")]
        public extern Lookup<TKey, TSource> ToLookup<TKey, TSource>(Func<TSource, TKey> keySelector,
            Func<TSource, TSource> elementSelector, IEqualityComparer<TKey> comparer);

        public extern EnumerableInstance<TElement> Trace();

        public extern EnumerableInstance<TElement> Trace(string message);

        public extern EnumerableInstance<TElement> Trace(string message, Func<TElement, string> selector);

        public extern EnumerableInstance<TElement> Union(IEnumerable<TElement> other);

        public extern EnumerableInstance<TElement> Union(IEnumerable<TElement> other, IEqualityComparer<TElement> comparer);

        public extern EnumerableInstance<TElement> Where(Func<TElement, bool> predicate);

        public extern EnumerableInstance<TElement> Where(Func<TElement, int, bool> predicate);

        [Template("{this}:zip({other}, {selector}, {TResult})")]
        public extern EnumerableInstance<TResult> Zip<TOther, TResult>(IEnumerable<TOther> other,
            Func<TElement, TOther, TResult> selector);

        public extern EnumerableInstance<TResult> Zip<TOther, TResult>(IEnumerable<TOther> other,
            Func<TElement, TOther, int, TResult> selector);
    }
}
