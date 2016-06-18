using Bridge;

namespace System
{
    [External]
    [Name("System.Object")]
    [IgnoreCast]
    public class Object
    {
        [Template("System.Object.toString({this})")]
        public virtual extern string ToString();

        [Template("System.getType({this})")]
        public virtual extern Type GetType();

        public static extern bool ReferenceEquals(object a, object b);

        [Template("System.Object.equals({this, {o}})")]
        public virtual extern bool Equals(object o);

        [Name("equalsStatic")]
        public static extern bool Equals(object a, object b);

        [Template("System.Object.getHashCode({this})")]
        public virtual extern int GetHashCode();
    }
}
