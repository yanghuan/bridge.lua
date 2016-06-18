using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Int")]
    [Constructor("Bridge.Int")]
    public struct Int64 : IComparable, IComparable<Int64>, IEquatable<Int64>, IFormattable
    {
        private Int64(int i)
        {
        }

        [InlineConst]
        public const long MinValue = -9007199254740991;

        [InlineConst]
        public const long MaxValue = 9007199254740991;

        [Template("Bridge.Int.parse({s}, -9007199254740991, 9007199254740991)")]
        public static long Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Int.parse({s}, -9007199254740991, 9007199254740991, {radix})")]
        public static long Parse(string s, int radix)
        {
            return 0;
        }

        [Template("Bridge.Int.tryParse({s}, {result}, -9007199254740991, 9007199254740991)")]
        public static bool TryParse(string s, out long result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Int.tryParse({s}, {result}, -9007199254740991, 9007199254740991, {radix})")]
        public static bool TryParse(string s, out long result, int radix)
        {
            result = 0;
            return false;
        }

        [Template("tostring({this})")]
        public override string ToString()
        {
            return null;
        }

        [Template("tostring({this})")]
        public string ToString(string format)
        {
            return null;
        }

        [Template("tostring({this})")]
        public string ToString(string format, IFormatProvider provider)
        {
            return null;
        }

        [Template("Bridge.Int.compareTo({this}, {other})")]
        public int CompareTo(long other)
        {
            return 0;
        }

        [Template("Bridge.Int.compareToObj({this}, {obj})")]
        public int CompareTo(object obj)
        {
            return 0;
        }

        [Template("Bridge.Int.equals({this}, {other})")]
        public bool Equals(Int64 other) {
            return false;
        }

        [Template("Bridge.Int.equalsObj({this}, {obj})")]
        public override bool Equals(object obj) {
            return false;
        }

        [Template("Bridge.Int.getHashCode({this})")]
        public override int GetHashCode() {
            return 0;
        }
    }
}
