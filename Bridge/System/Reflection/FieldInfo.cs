using Bridge;

namespace System.Reflection {
    [External]
    public abstract class FieldInfo {
        public abstract object GetValue(object obj);
    }
}