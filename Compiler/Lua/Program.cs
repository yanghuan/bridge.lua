using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Lua {
    class Program {
       private const string HelpCmdString = @"Usage: Bridge.Lua [-f srcfolder] [-p outfolder] [-l thridlibs]
Options and arguments 
-f              : intput directory, all *.cs files whill be compiled
-p              : out directory, will put the out lua files
-b [option]     : the path of bridge.all, defalut will use bridge.all under the same directory of Bridge.Lua.exe
-l [option]     : third-party libraries referenced, use ';' to separate
-lw [option]    : whitelist of third-party libraries, use ';' to separate           
-lb [option]    : blacklist of third-party libraries, use ';' to separate

Compiled successfully, and then will have a manifest file to the output directory named manifest.lua, use require(""manifest.lua"")(you_put_dir) to load all
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
                    string bridge = cmds.GetArgument("-b", true);
                    string lib = cmds.GetArgument("-l", true);
                    string libWhite = cmds.GetArgument("-lw", true);
                    string libBlack = cmds.GetArgument("-lb", true);
                    Worker w = new Worker(folder, output, bridge, lib, libWhite, libBlack);
                    w.Do();
                    Console.WriteLine("all operator success");
                }
                catch(CmdArgumentException e) {
                    Console.Error.WriteLine(e.ToString());
                    Environment.ExitCode = -1;
                }
                catch(BridgeLuaException e) {
                    Console.Error.WriteLine(e.Message);
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
