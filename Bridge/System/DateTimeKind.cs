using Bridge;

namespace System {

    [External]
    [Enum(Emit.Value)]
    public enum DateTimeKind {
        Unspecified = 0,
        Utc = 1,
        Local = 2,
    }
}
