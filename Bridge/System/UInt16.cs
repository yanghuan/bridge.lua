using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Int")]
    [Constructor("Bridge.Int")]
    public struct UInt16 : IComparable, IComparable<UInt16>, IEquatable<UInt16>, IFormattable
    {
        private UInt16(int i)
        {
        }

        [InlineConst]
        public const ushort MinValue = 0;

        [InlineConst]
        public const ushort MaxValue = 65535;

        [Template("Bridge.Int.parseInt({s}, 0, 65535)")]
        public static ushort Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Int.parseInt({s}, 0, 65535, {radix})")]
        public static ushort Parse(string s, int radix)
        {
            return 0;
        }

        [Template("Bridge.Int.tryParseInt({s}, {result}, 0, 65535)")]
        public static bool TryParse(string s, out ushort result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Int.tryParseInt({s}, {result}, 0, 65535, {radix})")]
        public static bool TryParse(string s, out ushort result, int radix)
        {
            result = 0;
            return false;
        }

        [Template("tostring({this})")]
        public override string ToString() {
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

        [Template("{this} - {other}")]
        public int CompareTo(ushort other)
        {
            return 0;
        }

        [Template("Bridge.Int.compareToObj({this}, {obj})")]
        public int CompareTo(object obj)
        {
            return 0;
        }

        [Template("Bridge.Int.equals({this}, {other})")]
        public bool Equals(ushort other)
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
