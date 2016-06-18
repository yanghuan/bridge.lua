using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Double")]
    [Constructor("Bridge.Double")]
    public struct Single : IComparable, IComparable<Single>, IEquatable<Single>, IFormattable
    {
        private Single(int i)
        {
        }

        [InlineConst]
        public const float MaxValue = (float)3.40282346638528859e+38;

        [InlineConst]
        public const float MinValue = (float)-3.40282346638528859e+38;

        [InlineConst]
        public const float Epsilon = (float)1.4e-45;

        [Template("0 / 0")]
        public static readonly float NaN = 0;

        [Template("-1 / 0")]
        public static readonly float NegativeInfinity = 0;

        [Template("1 / 0")]
        public static readonly float PositiveInfinity = 0;

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

        [Template("Bridge.Double.parse({s})")]
        public static float Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Double.parse({s}, {provider})")]
        public static float Parse(string s, IFormatProvider provider)
        {
            return 0;
        }

        [Template("Bridge.Double.tryParse({s}, null, {result})")]
        public static bool TryParse(string s, out float result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Double.tryParse({s}, {provider}, {result})")]
        public static bool TryParse(string s, IFormatProvider provider, out float result)
        {
            result = 0;
            return false;
        }

        public static bool IsPositiveInfinity(float d)
        {
            return false;
        }

        public static bool IsNegativeInfinity(float d)
        {
            return false;
        }

        public static bool IsInfinity(float d)
        {
            return false;
        }

        public static bool IsNaN(float d)
        {
            return false;
        }

        [Template("Bridge.Double.compareTo({this}, {other})")]
        public int CompareTo(float other)
        {
            return 0;
        }

        [Template("Bridge.Double.compareToObj({this}, {obj})")]
        public int CompareTo(object obj)
        {
            return 0;
        }

        [Template("Bridge.Double.equals({this}, {other})")]
        public bool Equals(float other)
        {
            return false;
        }

        [Template("Bridge.Double.equalsToObj({this}, {obj})")]
        public override bool Equals(object obj) {
            return false;
        }

        [Template("Bridge.Double.getHashCode({this})")]
        public override int GetHashCode() {
            return 0;
        }
    }
}
