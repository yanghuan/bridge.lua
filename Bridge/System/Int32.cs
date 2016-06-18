using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Int")]
    [Constructor("Bridge.Int")]
    public struct Int32 : IComparable, IComparable<Int32>, IEquatable<Int32>, IFormattable
    {
        private Int32(int i)
        {
        }

        [InlineConst]
        public const int MinValue = -2147483648;

        [InlineConst]
        public const int MaxValue = 2147483647;

        [Template("Bridge.Int.parse({s}, -2147483648, 2147483647)")]
        public static int Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Int.parse({s}, -2147483648, 2147483647, {radix})")]
        public static int Parse(string s, int radix)
        {
            return 0;
        }

        [Template("Bridge.Int.tryParse({s}, {result}, -2147483648, 2147483647)")]
        public static bool TryParse(string s, out int result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Int.tryParse({s}, {result}, -2147483648, 2147483647, {radix})")]
        public static bool TryParse(string s, out int result, int radix)
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
        public int CompareTo(int other)
        {
            return 0;
        }

        [Template("Bridge.Int.compareToObj({this}, {obj})")]
        public int CompareTo(object obj)
        {
            return 0;
        }

        [Template("Bridge.Int.equals({this}, {other})")]
        public bool Equals(int other)
        {
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
