using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.NRefactory.TypeSystem;

namespace Bridge.Contract {
    public static class TransformCtx {
        public const string DefaultString = "__defaultVal__";
        public const string DefaultInvoke = DefaultString + "()";

        public static T GetOrDefault<K, T>(this IDictionary<K, T> dict, K key, T t = default(T)) {
            T v;
            if(dict.TryGetValue(key, out v)) {
                return v;
            }
            return t;
        }

        public sealed class MethodInfo {
            public string Name;
            public bool IsPrivate;
            public bool IsCtor;
        }

        public static readonly HashSet<string> CurUsingNamespaces = new HashSet<string>();
        public static Func<IEntity, string> GetEntityName;
        public static List<MethodInfo> CurClassMethodNames = new List<MethodInfo>();
        public static IType CurClass;
        public static List<string> CurClassOtherMethods = new List<string>();
        public static List<MethodInfo> CurClassOtherMethodNames = new List<MethodInfo>();
        public static Dictionary<ITypeInfo, string> NamespaceNames = new Dictionary<ITypeInfo, string>();
        public static HashSet<IType> ExportEnums = new HashSet<IType>();
    }
}
