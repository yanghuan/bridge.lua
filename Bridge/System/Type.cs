using Bridge;
using System.Reflection;

namespace System {
    [External]
    public class Type {
        public static extern Type GetTypeFromHandle(RuntimeTypeHandle typeHandle);

        public string Name { get; }

        public string FullName { get; }

        public string Namespace { get; }

        public bool IsGenericType { get; }

        public bool IsInterface { get; }

        public Type BaseType { get; }

        public bool IsSubclassOf(Type c) {
            return false;
        }

        public bool IsAssignableFrom(Type c) {
            return false;
        }

        public bool IsInstanceOfType(Object o) {
            return false;
        }

        public Type[] GetInterfaces() {
            return null;
        }

        public PropertyInfo GetProperty(string name) {
            return null;
        }

        public FieldInfo GetField(string name) {
            return null;
        }
    }
}
