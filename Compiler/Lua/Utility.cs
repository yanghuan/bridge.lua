using Microsoft.CSharp;
using Mono.Cecil;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bridge.Lua {
    public sealed class CmdArgumentException : Exception {
        public CmdArgumentException(string message) : base(message) {
        }
    }

    public static class Utility {
        public static Dictionary<string, string[]> GetCommondLines(string[] args) {
            Dictionary<string, string[]> cmds = new Dictionary<string, string[]>();

            string key = "";
            List<string> values = new List<string>();

            foreach(string arg in args) {
                string i = arg.Trim();
                if(i.StartsWith("-")) {
                    if(!string.IsNullOrEmpty(key)) {
                        cmds.Add(key, values.ToArray());
                        key = "";
                        values.Clear();
                    }
                    key = i;
                }
                else {
                    values.Add(i);
                }
            }

            if(!string.IsNullOrEmpty(key)) {
                cmds.Add(key, values.ToArray());
            }
            return cmds;
        }

        public static T GetOrDefault<K, T>(this IDictionary<K, T> dict, K key, T t = default(T)) {
            T v;
            if(dict.TryGetValue(key, out v)) {
                return v;
            }
            return t;
        }

        public static string GetArgument(this Dictionary<string, string[]> args, string name, bool isOption = false) {
            string[] values = args.GetOrDefault(name);
            if(values == null || values.Length == 0) {
                if(isOption) {
                    return null;
                }
                throw new CmdArgumentException(name + " is not found");
            }
            return values[0];
        }

        public static string GetCurrentDirectory(string path) {
            const string CurrentDirectorySign1 = "~/";
            const string CurrentDirectorySign2 = "~\\";

            if(path.StartsWith(CurrentDirectorySign1)) {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.Substring(CurrentDirectorySign1.Length));
            }
            else if(path.StartsWith(CurrentDirectorySign2)) {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.Substring(CurrentDirectorySign2.Length));
            }

            return Path.Combine(Environment.CurrentDirectory, path);
        }

        public static string Move(string directory, string lib) {
            string path = Path.Combine(directory, Path.GetFileName(lib));
            File.Copy(lib, path, true);
            return path;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/2210309/how-to-find-out-if-a-property-is-an-auto-implemented-property-with-reflection
        /// </summary>
        public static bool IsAutoProperty(this PropertyDefinition prop) {
            return prop.DeclaringType.Fields.Any(f => f.IsPrivate && !f.IsStatic && f.Name.Contains("<" + prop.Name + ">"));
        }

        public static bool IsVolatile(this FieldDefinition fieldDefinition) {
            RequiredModifierType modifierType = fieldDefinition.FieldType as RequiredModifierType;
            if(modifierType != null && modifierType.ModifierType.FullName == "System.Runtime.CompilerServices.IsVolatile") {
                return true;
            }
            return false;
        }

        public static bool IsExtensionMethod(this MethodDefinition methodDefinition) {
            return methodDefinition.CustomAttributes.Any(i => i.AttributeType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute");
        }

        private static Regex genericInstanceRegex_ = new Regex(@"`\d+", RegexOptions.Compiled | RegexOptions.Singleline);

        public static string RemoveGenericInstanceSign(string name) {
            return genericInstanceRegex_.Replace(name, "");
        }
 
        public static string Compile(this CodeCompileUnit unit) {
            using(MemoryStream stream = new MemoryStream()) {
                StreamWriter sourceWriter = new StreamWriter(stream);
                CSharpCodeProvider provider = new CSharpCodeProvider();
                provider.GenerateCodeFromCompileUnit(unit, sourceWriter, new CodeGeneratorOptions());
                sourceWriter.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                StringBuilder sb = new StringBuilder();
                int lineNum = 0;
                StreamReader reader = new StreamReader(stream);
                while(true) {
                    string line = reader.ReadLine();
                    if(line != null) {
                        FixLineCode(ref line);
                        sb.AppendLine(line);
                    }
                    else {
                        break;
                    }
                    ++lineNum;
                }
                return sb.ToString();
            }
        }

        /*
        private static Regex fixLineCodeRegex_ = new Regex(@"sealed abstract|(\S+)\sop_Implicit@|(\S+)\sop_Explicit@", RegexOptions.Compiled | RegexOptions.Singleline);

        public static void FixLineCode(ref string lineCode) {
            lineCode = fixLineCodeRegex_.Replace(lineCode, m => {
                if(m.Value == "sealed abstract") {
                    return "static";
                }
                else if(m.Value.EndsWith("op_Implicit@")) {
                    return "implicit operator " + m.Groups[1].Value;
                }
                else if(m.Value.EndsWith("p_Explicit@")) {
                    return "explicit operator " + m.Groups[1].Value;
                }
                throw new Exception();
            });
        }*/

        public static void FixLineCode(ref string lineCode) {
            bool isChanged = FixStaticClass(ref lineCode);
            if(isChanged) {
                return;
            }

            isChanged = FixOpImplicit(ref lineCode);
            if(isChanged) {
                return;
            }

            FixOpExplicit(ref lineCode);
        }

        public static bool FixStaticClass(ref string lineCode) {
            string code = lineCode.Replace("sealed abstract", "static");
            if(code.Length != lineCode.Length) {
                lineCode = code;
                return true;
            }
            return false;
        }

        public static bool FixOpImplicit(ref string lineCode) {
            const string kSign = " op_Implicit@";
            int pos = lineCode.IndexOf(kSign);
            if(pos != -1) {
                int prev = lineCode.LastIndexOf(' ', pos - 1);
                if(prev != -1) {
                    string value = lineCode.Substring(prev + 1, pos - prev - 1);
                    lineCode = lineCode.Replace(value + kSign, "implicit operator " + value);
                    return true;
                }
            }
            return false;
        }

        public static bool FixOpExplicit(ref string lineCode) {
            const string kSign = " op_Explicit@";
            int pos = lineCode.IndexOf(kSign);
            if(pos != -1) {
                int prev = lineCode.LastIndexOf(' ', pos - 1);
                if(prev != -1) {
                    string value = lineCode.Substring(prev + 1, pos - prev - 1);
                    lineCode = lineCode.Replace(value + kSign, "explicit operator " + value);
                    return true;
                }
            }
            return false;
        }
    }
}
