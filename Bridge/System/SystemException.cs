using Bridge;

namespace System {
    [External]
    public class SystemException : Exception, IBridgeClass {
        public SystemException() {
        }

        public SystemException(string message) : base(message) {
        }

        public SystemException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
