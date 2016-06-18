using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Int")]
    [Constructor("Bridge.Int")]
    public struct Byte : IComparable, IComparable<Byte>, IEquatable<Byte>, IFormattable
    {
        private Byte(int i)
        {
        }

        [InlineConst]
        public const byte MinValue = 0;

        [InlineConst]
        public const byte MaxValue = 255;

        [Template("Bridge.Int.parse({s}, 0, 255)")]
        public static byte Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Int.parse({s}, 0, 255, {radix})")]
        public static byte Parse(string s, int radix)
        {
            return 0;
        }

        [Template("Bridge.Int.tryParse({s}, {result}, 0, 255)")]
        public static bool TryParse(string s, out byte result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Int.tryParse({s}, {result}, 0, 255, {radix})")]
        public static bool TryParse(string s, out byte result, int radix)
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
        public int CompareTo(byte other)
        {
            return 0;
        }

        [Template("Bridge.Int.compareToObj({this}, {obj})")]
        public int CompareTo(object obj)
        {
            return 0;
        }

        [Template("Bridge.Int.equals({this}, {other})")]
        public bool Equals(byte other)
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
