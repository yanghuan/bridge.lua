using Bridge;

namespace System
{
    /// <summary>
    /// Specifies how mathematical rounding methods should process a number that is midway between two numbers.
    /// </summary>
    [External]
    [Enum(Emit.Value)]
    public enum MidpointRounding
    {
        ToEven = 0,
        AwayFromZero = 1,
    }
}
