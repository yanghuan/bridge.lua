using Bridge;

namespace System {
    [External]
    [Name("Bridge.Boolean")]
    [Constructor("Bridge.Boolean")]
    public struct Boolean : IComparable, IComparable<Boolean>, IEquatable<Boolean> {

        [InlineConst]
        public const string FalseString = "false";
        [InlineConst]
        public const string TrueString = "true";

        [Template("Bridge.Boolean.compareTo({this}, {other})")]
        public int CompareTo(bool other) {
            return 0;
        }

        [Template("Bridge.Boolean.compareToObj({this}, {obj})")]
        public int CompareTo(object obj) {
            return 0;
        }

        [Template("Bridge.Boolean.equals({this}, {other})")]
        public bool Equals(bool other) {
            return false;
        }

        [Template("Bridge.Boolean.equalsObj({this}, {obj})")]
        public override bool Equals(object obj) {
            return false;
        }

        [Template("Bridge.Boolean.getHashCode({this})")]
        public override int GetHashCode() {
            return 0;
        }

        [Template("tostring({this})")]
        public override string ToString() {
            return null;
        }

        [Template("tostring({this})")]
        public string ToString(string format) {
            return null;
        }

        [Template("Bridge.Boolean.Parse({s})")]
        public static int Parse(string s) {
            return 0;
        }

        [Template("Bridge.Boolean.TryParse({s}, {result})")]
        public static bool TryParse(string s, out int result) {
            result = 0;
            return false;
        }
    }
}