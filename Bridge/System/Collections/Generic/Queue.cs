using Bridge;

namespace System.Collections.Generic {

    [External]
    [Namespace("Bridge")]
    [Name("System.Queue")]
    public class Queue<T> : ICollection, IBridgeClass {

        public Queue() {
        }

        public Queue(int capacity) {
        }

        public Queue(IEnumerable<T> collection) {
        }

        public int Count {
            [Template("#{this}")]
            get {
                return 0;
            }
        }

        public void Clear() {
        }

        public void Enqueue(T item) {
        }

        public T Dequeue() {
            return default(T);
        }

        public T Peek() {
            return default(T);
        }

        public bool Contains(T item) {
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return null;
        }

        public IEnumerator<T> GetEnumerator() {
            return null;
        }

        public T[] ToArray() {
            return null;
        }

        public void TrimExcess() {
        }
    }
}