using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Int")]
    [Constructor("Bridge.Int")]
    public struct Int16 : IComparable, IComparable<Int16>, IEquatable<Int16>, IFormattable
    {
        private Int16(int i)
        {
        }

        [InlineConst]
        public const short MinValue = -32768;

        [InlineConst]
        public const short MaxValue = 32767;

        [Template("Bridge.Int.parse({s}, -32768, 32767)")]
        public static short Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Int.parse({s}, -32768, 32767, {radix})")]
        public static short Parse(string s, int radix)
        {
            return 0;
        }

        [Template("Bridge.Int.tryParse({s}, {result}, -32768, 32767)")]
        public static bool TryParse(string s, out short result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Int.tryParse({s}, {result}, -32768, 32767, {radix})")]
        public static bool TryParse(string s, out short result, int radix)
        {
            result = 0;
            return false;
        }

        [Template("tostring(this)")]
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
        public int CompareTo(short other)
        {
            return 0;
        }

        [Template("Bridge.Int.compareToObj({this}, {obj})")]
        public int CompareTo(object obj)
        {
            return 0;
        }

        [Template("Bridge.Int.equals({this}, {other})")]
        public bool Equals(short other)
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
