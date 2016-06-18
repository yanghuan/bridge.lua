using Bridge;

namespace System
{
    [External]
    [Namespace("Bridge")]
    public interface IFormattable : IBridgeClass
    {
        [Template("Bridge.format({this}, {format}, {formatProvider})")]
        string ToString(string format, IFormatProvider formatProvider);
    }
}
