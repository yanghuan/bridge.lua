using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Lua {
    class Program {
       private const string HelpCmdString = @"Usage: Bridge.Lua [-f srcfolder] [-p outfolder]
Arguments 
-f              : intput directory, all *.cs files whill be compiled
-p              : out directory, will put the out lua files

Options
-b [option]     : the path of bridge.all, defalut will use bridge.all under the same directory of Bridge.Lua.exe
-l [option]     : third-party libraries referenced, use ';' to separate
-lb [option]    : blacklist of third-party libraries, use ';' to separate,
                  E.g '#System.Collections;System.Text.StringBuilder', except class named System.Text.StringBuilder, namespace named System.Collections
-lw [option]    : whitelist of third-party libraries, use ';' to separate           
";
        static void Main(string[] args) {
            if(args.Length > 0) {
                try {
                    var cmds = Utility.GetCommondLines(args);
                    if(cmds.ContainsKey("-h")) {
                        ShowHelpInfo();
                        return;
                    }

                    string folder = cmds.GetArgument("-f");
                    string output = cmds.GetArgument("-p");
                    string lib = cmds.GetArgument("-l", true);
                    Worker w = new Worker(folder, output, lib);
                    w.Do();
                    Console.WriteLine("all operator success");
                }
                catch(CmdArgumentException e) {
                    Console.Error.WriteLine(e.ToString());
                    ShowHelpInfo();
                    Environment.ExitCode = -1;
                }
                catch(Exception e) {
                    Console.Error.WriteLine(e.ToString());
                    Environment.ExitCode = -1;
                }
            }
            else {
                ShowHelpInfo();
                Environment.ExitCode = -1;
            }
        }

        private static void ShowHelpInfo() {
            Console.Error.WriteLine(HelpCmdString);
        }
    }
}
