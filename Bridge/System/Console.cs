using Bridge;

namespace System {
    [External]
    public static class Console {
        public extern static int Read();
        public extern static string ReadLine();
        public extern static void Write(object v);

        [Template("System.Console.write(string.char({v}))")]
        public extern static void Write(char v);

        [Template("System.Console.write(System.String({buffer}))")]
        public extern static void Write(char[] buffer);

        [Template("System.Console.write(System.String({buffer}, {index}, {count}))")]
        public extern static void Write(char[] buffer, int index, int count);

        public extern static void Write(string format, params object[] arg);

        public extern static void WriteLine(object v);

        [Template("System.Console.writeLine(string.char({v}))")]
        public extern static void WriteLine(char v);

        [Template("System.Console.writeLine(System.String({buffer}))")]
        public extern static void WriteLine(char[] buffer);

        [Template("System.Console.writeLine(System.String({buffer}, {index}, {count}))")]
        public extern static void WriteLine(char[] buffer, int index, int count);

        public extern static void WriteLine(string format, params object[] arg);
    }
}
