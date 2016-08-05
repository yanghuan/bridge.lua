using Bridge;

namespace System.Diagnostics.Contracts {
    public static class Contract {
        [Template("assert({condition})")]
        public static void Assert(bool condition) {
        }

        [Template("assert({condition}, {userMessage})")]
        public static void Assert(bool condition, string userMessage) {
        }

        [Template("assert({condition})")]
        public static void Assume(bool condition) {
        }

        [Template("assert({condition}, {userMessage})")]
        public static void Assume(bool condition, string userMessage) {
        }

        [Template("assert({condition})")]
        public static void Ensures(bool condition) {
        }

        [Template("assert({condition}, {userMessage})")]
        public static void Ensures(bool condition, string userMessage) {
        }
    }
}