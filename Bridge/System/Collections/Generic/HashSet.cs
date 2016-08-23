using Bridge;

namespace System.Collections.Generic {

    [External]
    [Namespace("Bridge")]
    [Name("System.HashSet")]
    public class HashSet<T> : ICollection<T>, ISet<T>, IBridgeClass {

        public HashSet() {
        }

        public HashSet(IEqualityComparer<T> camparer) {
        }

        public HashSet(IEnumerable<T> collection) {
        }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> camparer) {
        }

        public void Clear() {
        }

        public bool Contains(T item) {
            return false;
        }

        public bool Remove(T item) {
            return false;
        }

        public int Count {
            get {
                return 0;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return null;
        }

        public IEnumerator<T> GetEnumerator() {
            return null;
        }

        void ICollection<T>.Add(T item) {
        }

        public bool Add(T item) {
            return false;
        }

        public void UnionWith(IEnumerable<T> other) {
        }

        public void IntersectWith(IEnumerable<T> other) {
        }

        public void ExceptWith(IEnumerable<T> other) {
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            return false;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            return false;
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            return false;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            return false;
        }

        public bool Overlaps(IEnumerable<T> other) {
            return false;
        }

        public bool SetEquals(IEnumerable<T> other) {
            return false;
        }

        public int RemoveWhere(Predicate<T> match) {
            return 0;
        }

        public void TrimExcess() {
        }
    }
}