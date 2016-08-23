using Bridge;

namespace System.Collections.Generic {

    [External]
    [Namespace("Bridge")]
    [Name("System.LinkedList")]
    public class LinkedList<T> : ICollection<T>, ICollection, IBridgeClass {
        public LinkedList() {
        }
        public LinkedList(IEnumerable<T> collection) {
        }

        public int Count {
            get {
                return 0;
            }
        }

        public LinkedListNode<T> First {
            get { return null; }
        }

        public LinkedListNode<T> Last {
            get { return null; }
        }

        void ICollection<T>.Add(T value) {
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value) {
            return null;
        }

        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode) {
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value) {
            return null;
        }

        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode) {
        }

        public LinkedListNode<T> AddFirst(T value) {
            return null;
        }

        public void AddFirst(LinkedListNode<T> node) {
        }

        public LinkedListNode<T> AddLast(T value) {
            return null;
        }

        public void AddLast(LinkedListNode<T> node) {
        }

        public void Clear() {
        }

        public bool Contains(T value) {
            return false;
        }

        public LinkedListNode<T> Find(T value) {
            return null;
        }

        public LinkedListNode<T> FindLast(T value) {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return null;
        }

        public IEnumerator<T> GetEnumerator() {
            return null;
        }

        public bool Remove(T value) {
            return false;
        }

        public void Remove(LinkedListNode<T> node) {
        }

        public void RemoveFirst() {
        }

        public void RemoveLast() {
        }
    }

    public sealed class LinkedListNode<T> {
        public LinkedListNode<T> Next {
            get { return null; }
        }

        public LinkedListNode<T> Previous {
            get {
                return null;
            }
        }

        public T Value { get; set; }
        public LinkedList<T> List { get; private set; }
    }
}