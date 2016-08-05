using Bridge;

namespace System.Diagnostics {
    public static class Debug {
        [Template("assert({condition})")]
        public static void Assert(bool condition) {
        }

        [Template("assert({condition}, {userMessage})")]
        public static void Assert(bool condition, string userMessage) {
        }
    }
}