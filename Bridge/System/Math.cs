using Bridge;

namespace System
{
    [External]
    [Name("math")]
    public static class Math
    {
        [InlineConst]
        public const double E = 2.7182818284590452354;

        [InlineConst]
        public const double PI = 3.14159265358979323846;

        public static extern int Abs(int x);

        public static extern double Abs(double x);

        public static extern double Abs(long x);

        [Template("{l}.abs()")]
        public static extern decimal Abs(decimal l);

        public static extern int Max(params int[] values);

        public static extern double Max(params double[] values);

        public static extern double Max(params long[] values);

        public static extern double Max(params ulong[] values);

        [Template("System.Decimal.max({*values})")]
        public static extern decimal Max(params decimal[] values);

        public static extern int Min(params int[] values);

        public static extern double Min(params double[] values);

        public static extern double Min(params long[] values);

        public static extern double Min(params ulong[] values);

        [Template("System.Decimal.min({*values})")]
        public static extern decimal Min(params decimal[] values);

        public static extern double Sqrt(double x);

        [Template("{d}.ceil()")]
        public static extern decimal Ceiling(decimal d);

        [Name("ceil")]
        public static extern double Ceiling(double d);

        public static extern double Floor(double x);

        [Template("{d}.floor()")]
        public static extern decimal Floor(decimal d);

        [Template("System.Decimal.round({x}, 6)")]
        public static extern decimal Round(decimal x);

        public static extern double Round(double d);

        [Template("System.Decimal.toDecimalPlaces({d}, {digits}, 6)")]
        public static extern decimal Round(decimal d, int digits);

        public static extern double Round(double d, int digits);

        [Template("System.Decimal.round({d}, {method})")]
        public static extern decimal Round(decimal d, MidpointRounding method);

        [Template("math.round({d}, 0, {method})")]
        public static extern double Round(double d, MidpointRounding method);

        [Template("System.Decimal.toDecimalPlaces({d}, {digits}, {method})")]
        public static extern decimal Round(decimal d, int digits, MidpointRounding method);

        public static extern double Round(double d, int digits, MidpointRounding method);

        [Template("{x} - ({y} * Math.round({x} / {y}))")]
        public static extern double IEEERemainder(double x, double y);

        public static extern double Exp(double x);

        [Template("math.log({x}, 2.7182818284590452354)")]
        public static extern double Log(double x);

        public static extern double Log(double x, double newBase);

        [Template("({x} ^ {y})")]
        public static extern double Pow(double x, double y);

        public static extern double Acos(double x);

        public static extern double Asin(double x);

        public static extern double Atan(double x);

        public static extern double Atan2(double y, double x);

        public static extern double Cos(double x);

        public static extern double Sin(double x);

        public static extern double Tan(double x);

        [Name("trunc")]
        public static extern double Truncate(double d);

        [Template("{d}.trunc()")]
        public static extern decimal Truncate(decimal d);

        public static extern int Sign(double value);

        [Template("{value}.sign()")]
        public static extern int Sign(decimal value);

        public static extern int DivRem(int a, int b, out int result);

        public static extern long DivRem(long a, long b, out long result);
    }
}
