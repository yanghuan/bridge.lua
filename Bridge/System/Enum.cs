using Bridge;

namespace System
{
    [External]
    [Name("System.Enum")]
    public abstract class Enum : ValueType {

        [Template("System.Enum.compareTo({this}, {target})")]
        public int CompareTo(object target) {
            return 0;
        }

        [Template("System.Enum.equalsToObj({this, {obj}})")]
        public override bool Equals(object obj) {
            return false;
        }

        [Template("System.Enum.getHashCode({this})")]
        public override int GetHashCode() {
            return 0;
        }

        [Template("System.Enum.toString({this}, {class})")]
        [EnumExport]
        public override string ToString() {
            return null;
        }

        [Template("System.typeof({class}, {this})")]
        [EnumExport]
        public override Type GetType() {
            return null;
        }

        public static extern Enum Parse(Type enumType, string value);

        public static extern Enum Parse(Type enumType, string value, bool ignoreCase);

        public static extern string ToString(Type enumType, Enum value);

        public static extern Array GetValues(Type enumType);

        public static extern string Format(Type enumType, object value, string format);

        public static extern string GetName(Type enumType, object value);

        public static extern string[] GetNames(Type enumType);

        [Template("System.Enum.hasFlag({this}, {flag})")]
        public extern bool HasFlag(Enum flag);

        public static extern bool IsDefined(Type enumType, object value);

        [Template("System.Enum.tryParse({TEnum}, {value}, {result})")]
        [EnumExport("TEnum")]
        public static extern bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct;

        [Template("System.Enum.tryParse({TEnum}, {value}, {result}, {ignoreCase})")]
        [EnumExport("TEnum")]
        public static extern bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct;
    }
}
