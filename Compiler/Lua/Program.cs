using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Lua {
    class Program {
        static void Main(string[] args) {
            try {
                var cmds = Utility.GetCommondLines(args);
                string folder = cmds.GetArgument("-f");
                string output = cmds.GetArgument("-p");
                string bridge = cmds.GetArgument("-b");
                string lib = cmds.GetArgument("-lib");
                Worker w = new Worker(folder, output, bridge, lib);
                w.Do();
                Console.WriteLine("all operator success");
            }
            catch(Exception e) {
                Console.Error.WriteLine("has error");
                Console.Error.WriteLine(e.ToString());
                Environment.ExitCode = -1;
            }

            Console.ReadKey();
        }
    }
}
