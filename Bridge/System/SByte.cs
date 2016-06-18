using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Int")]
    [Constructor("Bridge.Int")]
    public struct SByte : IComparable, IComparable<SByte>, IEquatable<SByte>, IFormattable
    {
        private SByte(int i)
        {
        }

        [InlineConst]
        public const sbyte MinValue = -128;

        [InlineConst]
        public const sbyte MaxValue = 127;

        [Template("Bridge.Int.parseInt({s}, -128, 127)")]
        public static sbyte Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Int.parseInt({s}, -128, 127, {radix})")]
        public static sbyte Parse(string s, int radix)
        {
            return 0;
        }

        [Template("Bridge.Int.tryParseInt({s}, {result}, -128, 127)")]
        public static bool TryParse(string s, out sbyte result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Int.tryParseInt({s}, {result}, -128, 127, {radix})")]
        public static bool TryParse(string s, out sbyte result, int radix)
        {
            result = 0;
            return false;
        }

        [Template("tostring({this})")]
        public override string ToString() {
            return null;
        }

        [Template("tostring({this})")]
        public string ToString(string format) {
            return null;
        }

        [Template("tostring({this})")]
        public string ToString(string format, IFormatProvider provider) {
            return null;
        }

        [Template("Bridge.Int.compareTo({this}, {other})")]
        public int CompareTo(sbyte other)
        {
            return 0;
        }

        [Template("Bridge.Int.compareToObj({this}, {obj})")]
        public int CompareTo(object obj)
        {
            return 0;
        }

        [Template("Bridge.Int.equals({this}, {other})")]
        public bool Equals(sbyte other)
        {
            return false;
        }

        [Template("Bridge.Int.equalsToObj({this}, {obj})")]
        public override bool Equals(object obj) {
            return false;
        }

        [Template("Bridge.Int.getHashCode({this})")]
        public override int GetHashCode() {
            return 0;
        }
    }
}
