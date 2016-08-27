using Bridge;
using System.Reflection;

namespace System {
    [External]
    public class Type {
        public static extern Type GetTypeFromHandle(RuntimeTypeHandle typeHandle);

        public string Name {
            get {
                return null;
            }
        }

        public string FullName {
            get {
                return null;
            }
        }

        public string Namespace {
            get {
                return null;
            }
        }

        public bool IsGenericType {
            get {
                return false;
            }
        }

        public bool IsInterface {
            get {
                return false;
            }
        }

        public Type BaseType {
            get {
                return null;
            }
        }

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
