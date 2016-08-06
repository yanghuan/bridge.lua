using Bridge;

namespace System.Reflection {
    [External]
    public abstract class PropertyInfo {
        public virtual object GetValue(object obj, object[] index) {
            return null;
        }
    }
}
