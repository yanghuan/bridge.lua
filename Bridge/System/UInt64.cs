using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Int")]
    [Constructor("Bridge.Int")]
    public struct UInt64 : IComparable, IComparable<UInt64>, IEquatable<UInt64>, IFormattable
    {
        private UInt64(int i)
        {
        }

        [InlineConst]
        public const ulong MinValue = 0;

        [InlineConst]
        public const ulong MaxValue = 9007199254740991;

        [Template("Bridge.Int.parseInt({s}, 0, 9007199254740991)")]
        public static ulong Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Int.parseInt({s}, 0, 9007199254740991, {radix})")]
        public static ulong Parse(string s, int radix)
        {
            return 0;
        }

        [Template("Bridge.Int.tryParseInt({s}, {result}, 0, 9007199254740991)")]
        public static bool TryParse(string s, out ulong result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Int.tryParseInt({s}, {result}, 0, 9007199254740991, {radix})")]
        public static bool TryParse(string s, out ulong result, int radix)
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
        public int CompareTo(ulong other)
        {
            return 0;
        }

        [Template("Bridge.Int.compareToObj({this}, {obj})")]
        public int CompareTo(object obj)
        {
            return 0;
        }

        [Template("Bridge.Int.equals({this}, {other})")]
        public bool Equals(ulong other)
        {
            return false;
        }

        [Template("Bridge.Int.equalsObj({this}, {obj})")]
        public override bool Equals(object o) {
            return false;
        }

        [Template("Bridge.Int.getHashCode({this})")]
        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
