using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Int")]
    [Constructor("Bridge.Int")]
    public struct UInt32 : IComparable, IComparable<UInt32>, IEquatable<UInt32>, IFormattable
    {
        private UInt32(int i)
        {
        }

        [InlineConst]
        public const uint MinValue = 0;

        [InlineConst]
        public const uint MaxValue = 4294967295;

        [Template("Bridge.Int.parseInt({s}, 0, 4294967295)")]
        public static uint Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Int.parseInt({s}, 0, 4294967295, {radix})")]
        public static uint Parse(string s, int radix)
        {
            return 0;
        }

        [Template("Bridge.Int.tryParseInt({s}, {result}, 0, 4294967295)")]
        public static bool TryParse(string s, out uint result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Int.tryParseInt({s}, {result}, 0, 4294967295, {radix})")]
        public static bool TryParse(string s, out uint result, int radix)
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
        public int CompareTo(uint other)
        {
            return 0;
        }

        [Template("Bridge.Int.compareToObj({this}, {obj})")]
        public int CompareTo(object obj)
        {
            return 0;
        }

        [Template("Bridge.Int.equals({this}, {other})")]
        public bool Equals(uint other)
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
