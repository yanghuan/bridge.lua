using Bridge;

namespace System.Diagnostics {
    [External]
    [Name("System.StackTrace")]
    public class StackTrace {
        public StackTrace() { }

        public StackTrace(bool fNeedFileInfo) { }

        public StackTrace(int skipFrames) { }

        public StackTrace(Exception e) { }

        public StackTrace(StackFrame frame) { }

        public StackTrace(int skipFrames, bool fNeedFileInfo) { }

        public StackTrace(Exception e, bool fNeedFileInfo) { }

        public StackTrace(Exception e, int skipFrames) { }

        public StackTrace(Exception e, int skipFrames, bool fNeedFileInfo) { }

        public virtual int FrameCount {
            get {
                return 0;
            }
        }

        public virtual StackFrame GetFrame(int index) {
            return null;
        }

        public virtual StackFrame[] GetFrames() {
            return null;
        }

        public override string ToString() {
            return null;
        }
    }
}
