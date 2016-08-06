using Bridge;

namespace System.Diagnostics {
    [External]
    [Name("System.StackFrame")]
    public class StackFrame {
        public StackFrame() { }

        public StackFrame(bool fNeedFileInfo) { }

        public StackFrame(int skipFrames) { }

        public StackFrame(int skipFrames, bool fNeedFileInfo) { }

        public StackFrame(string fileName, int lineNumber) { }

        public StackFrame(string fileName, int lineNumber, int colNumber) { }

        public virtual int GetFileColumnNumber() {
            return 0;
        }

        public virtual int GetFileLineNumber() {
            return 0;
        }

        public virtual string GetFileName() {
            return null;
        }

        public virtual int GetILOffset() {
            return 0;
        }

        public virtual int GetNativeOffset() {
            return 0;
        }

        public override string ToString() {
            return null;
        }
    }
}
