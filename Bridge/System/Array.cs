using Bridge;
using System.Collections;
using System.Collections.Generic;

namespace System
{
    [External]
    [Namespace("Bridge")]
    public sealed class Array : IEnumerable, ICloneable
    {
        [Template("#{this}")]
        public readonly int Length = 0;

        private Array()
        {
        }

        public extern IEnumerator GetEnumerator();

        public extern object GetValue(params int[] indices);

        public extern void SetValue(object value, params int[] indices);

        public extern int GetLength(int dimension);

        public int Rank
        {
            get
            {
                return 0;
            }
        }

        public object Clone() {
            return null;
        }

        #region array

        public static T[] Empty<T>() {
            return null;
        }

        public static bool Exists<T>(T[] array, Predicate<T> match) {
            return false;
        }

        public static T Find<T>(T[] array, Predicate<T> match) {
            return default(T);
        }

        public static T[] FindAll<T>(T[] array, Predicate<T> match) {
            return null;
        }

        public static int FindIndex<T>(T[] array, Predicate<T> match) {
            return 0;
        }

        public static int FindIndex<T>(T[] array, int startIndex, Predicate<T> match) {
            return 0;
        }

        public static int FindIndex<T>(T[] array, int startIndex, int count, Predicate<T> match) {
            return 0;
        }

        public static T FindLast<T>(T[] array, Predicate<T> match) {
            return default(T);
        }

        public static int FindLastIndex<T>(T[] array, Predicate<T> match) {
            return 0;
        }

        public static int FindLastIndex<T>(T[] array, int startIndex, Predicate<T> match) {
            return 0;
        }

        public static int FindLastIndex<T>(T[] array, int startIndex, int count, Predicate<T> match) {
            return 0;
        }

        public static void ForEach<T>(T[] array, Action<T> action) {
        }

        public static int IndexOf<T>(T[] array, T value) {
            return 0;
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex) {
            return 0;
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex, int count) {
            return 0;
        }

        public static int LastIndexOf<T>(T[] array, T value) {
            return 0;
        }

        public static int LastIndexOf<T>(T[] array, T value, int startIndex) {
            return 0;
        }

        public static int LastIndexOf<T>(T[] array, T value, int startIndex, int count) {
            return 0;
        }

        public static void Reverse<T>(T[] array) {
            Array.Reverse(array);
        }

        public static void Reverse<T>(T[] array, int index, int length) {
        }

        public static void Sort<T>(T[] array) { }

        public static void Sort<T>(T[] array, IComparer<T> comparer) { }

        public static void Sort<T>(T[] array, Comparison<T> comparison) {
        }

        public static void Sort<T>(T[] array, int index, int length, IComparer<T> comparer) { }

        public static void Sort<T>(T[] array, int index, int length) { }

        public static bool TrueForAll<T>(T[] array, Predicate<T> match) {
            return false;
        }


        #endregion
    }
}
