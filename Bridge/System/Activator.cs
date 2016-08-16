using Bridge;

namespace System
{
    [External]
    public static class Activator
    {
        [Template("{T}({*arguments})")]
        public static extern T CreateInstance<T>(params object[] arguments);

        [Template("{T}()")]
        public static extern T CreateInstance<T>();

        [Template("System.CreateInstance({type})")]
        public static extern object CreateInstance(Type type);

        [Template("System.CreateInstance({type}, ({*arguments})")]
        public static extern object CreateInstance(Type type, params object[] arguments);
    }
}
