using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Lua {
    class Program {
       private const string HelpCmdString = @"Usage: Bridge.Lua [-s srcfolder] [-d dstfolder]
Arguments 
-s              : source directory, all *.cs files whill be compiled
-d              : destination  directory, will put the out lua files

Options
-l [option]     : libraries referenced, use ';' to separate      
-m [option]     : meta files, like System.xml, use ';' to separate     
-h [option]     : show the help message    
";
        static void Main(string[] args) {
            if(args.Length > 0) {
                try {
                    var cmds = Utility.GetCommondLines(args);
                    if(cmds.ContainsKey("-h")) {
                        ShowHelpInfo();
                        return;
                    }

                    string folder = cmds.GetArgument("-s");
                    string output = cmds.GetArgument("-d");
                    string lib = cmds.GetArgument("-l", true);
                    string meta = cmds.GetArgument("-m", true);
                    Worker w = new Worker(folder, output, lib, meta);
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
