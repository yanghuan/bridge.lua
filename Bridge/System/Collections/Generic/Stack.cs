using Bridge;

namespace System.Collections.Generic {

    [External]
    [Namespace("Bridge")]
    [Name("System.Stack")]
    public class Stack<T> : ICollection, IBridgeClass {

        public Stack() {
        }

        public Stack(int capacity) {
        }

        public Stack(IEnumerable<T> collection) {
        }

        public int Count {
            [Template("#{this}")]
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

        public void Clear() {
        }

        public bool Contains(T item) {
            return false;
        }

        public void TrimExcess() {
        }

        public T Peek() {
            return default(T);
        }

        public T Pop() {
            return default(T);
        }

        public void Push(T item) {
        }

        public T[] ToArray() {
            return null;
        }
    }
}