using Bridge;

namespace System
{
    [External]
    [Name("Bridge.Double")]
    [Constructor("Bridge.Double")]
    public struct Double : IComparable, IComparable<Double>, IEquatable<Double>, IFormattable
    {
        private Double(int i)
        {
        }

        [InlineConst]
        public const double MinValue = -1.7976931348623157E+308;

        [InlineConst]
        public const double MaxValue = 1.7976931348623157E+308;

        [InlineConst]
        public const double Epsilon = 4.94065645841247E-324;

        [Template("0 / 0")]
        public static readonly double NaN = 0;

        [Template("-1 / 0")]
        public static readonly double NegativeInfinity = 0;

        [Template("1 / 0")]
        public static readonly double PositiveInfinity = 0;

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
 
        [Template("Bridge.Double.parse({s})")]
        public static double Parse(string s)
        {
            return 0;
        }

        [Template("Bridge.Double.parse({s}, {provider})")]
        public static double Parse(string s, IFormatProvider provider)
        {
            return 0;
        }

        [Template("Bridge.Double.tryParse({s}, null, {result})")]
        public static bool TryParse(string s, out double result)
        {
            result = 0;
            return false;
        }

        [Template("Bridge.Double.tryParse({s}, {provider}, {result})")]
        public static bool TryParse(string s, IFormatProvider provider, out double result)
        {
            result = 0;
            return false;
        }

        public static bool IsPositiveInfinity(double d)
        {
            return false;
        }

        public static bool IsNegativeInfinity(double d)
        {
            return false;
        }

        public static bool IsInfinity(double d)
        {
            return false;
        }

        public static bool IsNaN(double d)
        {
            return false;
        }

        [Template("Bridge.Double.compareTo({this}, {other})")]
        public int CompareTo(double other) {
            return 0;
        }

        [Template("Bridge.Double.compareToObj({this}, {obj})")]
        public int CompareTo(object obj) {
            return 0;
        }

        [Template("Bridge.Double.equals({this}, {other})")]
        public bool Equals(double other) {
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
