using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;

using Microsoft.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Newtonsoft.Json;
using Mono.Cecil;

using Bridge.Translator.Logging;
using Bridge.Contract;
using Bridge.Translator;

namespace Bridge.Lua {
    public sealed class Worker {
        private const string kBridge = "Bridge";
        private const string kTempDirName = "__temp__";
        private const string kOutDllName = "__out__.dll";
        private static readonly UTF8Encoding UTF8Encoding = new UTF8Encoding(false);
        private const string kBaseDll = "mscorlib.dll";
        private static readonly string[] SystemDlls = new string[] {
            "System.dll",
            "System.Core.dll",
        };

        private string folder_;
        private string output_;
        private string[] libs_;
        private string tempDirectory_;

        public Worker(string folder, string output, string lib) {
            folder_ = folder;
            output_ = output;
            if(!string.IsNullOrEmpty(lib)) {
                List<string> list = new List<string>();
                string[] libs = lib.Split(';');
                foreach(string path in libs) {
                    if(path.EndsWith(".dll")) {
                        list.Add(Utility.GetCurrentDirectory(path));
                    }
                }
                libs_ = list.ToArray();
            }
        }

        public void Do() {
            try {
                tempDirectory_ = Path.Combine(output_, kTempDirName);
                if(!Directory.Exists(tempDirectory_)) {
                    Directory.CreateDirectory(tempDirectory_);
                }

                string dllPath = Compiler();
                ToLua(dllPath);
            }
            finally {
                if(tempDirectory_ != null) {
                    if(Directory.Exists(tempDirectory_)) {
                        Directory.Delete(tempDirectory_, true);
                    }
                }
            }
        }

        private string Compiler() {
            string[] files = Directory.GetFiles(folder_, "*.cs", SearchOption.AllDirectories);
            if(files.Length == 0) {
                throw new ArgumentException(string.Format("{0} is empty", folder_));
            }
        
            CompilerParameters cp = new CompilerParameters();
            cp.CoreAssemblyFileName = kBaseDll;
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = false;
            cp.TreatWarningsAsErrors = false;
            cp.OutputAssembly = Path.Combine(tempDirectory_, kOutDllName);
            cp.ReferencedAssemblies.AddRange(SystemDlls);
            if(libs_ != null) {
                cp.ReferencedAssemblies.AddRange(libs_);
            }

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults cr = provider.CompileAssemblyFromFile(cp, files);
            if(cr.Errors.Count > 0) {
                StringBuilder sb = new StringBuilder();
                foreach(CompilerError ce in cr.Errors) {
                    sb.AppendFormat(" {0}", ce.ToString());
                    sb.AppendLine();
                }
                throw new System.Exception(sb.ToString());
            }

            return cr.PathToAssembly;
        }

        private string[] GetSearchPaths() {
            List<string> paths = new List<string>();
            paths.Add(RuntimeEnvironment.GetRuntimeDirectory());
            if(libs_ != null) {
                paths.AddRange(libs_.Select(Path.GetDirectoryName));
            }
            return paths.Distinct().ToArray();
        }

        private sealed class BridgeConfig {
            public string output;
            public bool autoPropertyToField = true;
            public string outputBy = "ClassPath";
            public string fileNameCasing = "None";
            public bool cutScripts = true;
        }

        private sealed class LuaTranslater : Bridge.Translator.Translator {
            private string output_;

            public LuaTranslater(string folder, string output, string dll) : base(folder, null, true, dll) {
                output_ = output;
                this.Log = new Logger("LuaTranslater", true, new ConsoleLoggerWriter());
                TransformCtx.GetEntityName = GetEntityName;
            }

            protected override IAssemblyInfo ReadConfig() {
                BridgeConfig bridgeJson = new BridgeConfig() { output = output_ };
                string json = JsonConvert.SerializeObject(bridgeJson);
                return JsonConvert.DeserializeObject<AssemblyInfo>(json);
            }

            private string GetEntityName(IEntity entity) {
                if(entity.SymbolKind == SymbolKind.Field) {
                    if(entity.DeclaringTypeDefinition.DirectBaseTypes.First().FullName == "ProtoBuf.IExtensible") {
                        return entity.Name.TrimStart('_');
                    }
                }
                return null;
            }
        }

        private void ToLua(string outDllPath) {
            var translator = new LuaTranslater(folder_, output_, outDllPath);
            translator.SearchPaths = GetSearchPaths();
            translator.XmlMetaFiles = new string[] { "System.xml" };
            translator.Translate();
            Save(translator);
        }

        private sealed class LuaFileInfo {
            private sealed class TypeInfo {
                public ITypeInfo Type { get; private set; }
                public string Content { get; private set; }
                public string Namespace { get; private set; }

                public TypeInfo(ITypeInfo type, string content) {
                    Type = type;
                    Content = content;
                    Namespace = TransformCtx.NamespaceNames[type];
                }
            }
            private const string kSystem = "System";

            private string sourceFileName_;
            private StringBuilder sb_ = new StringBuilder();
            public string OutPath { get; private set; }

            private HashSet<string> usings_ = new HashSet<string>();
            private List<TypeInfo> types_ = new List<TypeInfo>();

            public LuaFileInfo(string sourceFileName) {
                sourceFileName_ = sourceFileName;
                sb_.AppendFormat("local {0} = {1}", kSystem, kSystem);
                sb_.AppendLine();
            }

            public void AddType(ITypeInfo type, string content) {
                types_.Add(new TypeInfo(type, content));
            }

            public void AddUsingDeclaration(string[] names) {
                foreach(string i in names) {
                    usings_.Add(i);
                }
            }

            private string GetName(string s) {
                string name = s.Replace(".", "");
                if(name == s) {
                    name = '_' + name;
                }
                return name;
            }

            private void WriteUsingDeclaration() {
                bool useLinq = types_.Any(i => TransformCtx.UseLinqClasses.Contains(i.Type.Type));
                if(useLinq) {
                    sb_.AppendLine("local Linq = System.Linq");
                }
                usings_.Remove(kSystem);
                if(usings_.Count > 0) {
                    List<string> usings = usings_.ToList();
                    usings.Sort();

                    foreach(string i in usings) {
                        sb_.AppendFormat("local {0}", GetName(i));
                        sb_.AppendLine();
                    }

                    sb_.AppendLine();

                    sb_.AppendLine("System.usingDeclare(function()");
                    foreach(string i in usings) {
                        sb_.AppendFormat("    {0} = {1}", GetName(i), i);
                        sb_.AppendLine();
                    }

                    sb_.AppendLine("end)");
                }
            }

            private void WriteTypes() {
                var groups = types_.GroupBy(i => i.Namespace);
                foreach(var group in groups) {
                    if(group != groups.First()) {
                        sb_.AppendLine();
                    }
                    string namespaceStr = group.Key;
                    sb_.AppendFormat("System.namespace(\"{0}\", function(namespace)", namespaceStr);
                    sb_.AppendLine();
                    foreach(TypeInfo type in group) {
                        sb_.Append(type.Content);
                    }
                    sb_.Append("end)");
                }
            }

            public void Save(string sourceFolder, string outputFolder) {
                WriteUsingDeclaration();
                WriteTypes();

                string path = sourceFileName_.Remove(0, sourceFolder.Length).TrimStart(Path.DirectorySeparatorChar, '/');
                string extend = Path.GetExtension(path);
                path = path.Remove(path.Length - extend.Length, extend.Length);
                path = path.Replace('.', '_');
                string luaFileName = Path.Combine(outputFolder, path + ".lua");
                string dir = Path.GetDirectoryName(luaFileName);
                if(!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(luaFileName, sb_.ToString(), UTF8Encoding);
                OutPath = path;
            }
        }

        private bool IsTypeEnable(ITypeInfo type) {
            if(type.IsObjectLiteral) {
                return false;
            }

            if(type.IsEnum) {
                return TransformCtx.ExportEnums.Contains(type.Type);
            }

            return true;
        }

        private IEnumerable<ITypeInfo> GetEnableTypeInfos(List<ITypeInfo> types) {
            return types.Where(IsTypeEnable);
        }

        private string GetFileName(ITypeInfo typeInfo) {
            return ((dynamic)typeInfo).Type.Region.FileName;
        }

        private void Save(Bridge.Translator.Translator translator) {
            Dictionary<string, LuaFileInfo> luaFiles = translator.ParsedSourceFiles.ToDictionary(i => i.FileName, i => new LuaFileInfo(i.FileName));

            var usings = Bridge.Translator.Lua.EmitBlock.UsingNamespaces;
            foreach(var pair in usings) {
                string fileName = GetFileName(pair.Key);
                luaFiles[fileName].AddUsingDeclaration(pair.Value);
            }

            var types = GetEnableTypeInfos(translator.Types);
            foreach(var type in types) {
                string content = translator.Outputs[type.Key];  
                string fileName = GetFileName(type);
                luaFiles[fileName].AddType(type, content);
            }

            foreach(LuaFileInfo luaFileInfo in luaFiles.Values) {
                luaFileInfo.Save(folder_, output_);
            }

            SaveManifests(luaFiles.Values.ToList(), types);
        }

        public static string ToCorrectTypeName(string keyName) {
            return BridgeTypes.ConvertName(keyName);
        }

        private void SaveManifests(List<LuaFileInfo> luaFiles, IEnumerable<ITypeInfo> types) {
            luaFiles.Sort((x, y) => x.OutPath.CompareTo(y.OutPath));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("return function(dir)");
            sb.AppendLine("    dir = (dir and #dir > 0) and (dir .. '.') or \"\"");
            sb.AppendLine("    local require = require");
            sb.AppendLine("    local load = function(module) return require(dir .. module) end");
            sb.AppendLine();

            foreach(LuaFileInfo info in luaFiles) {
                string module = info.OutPath.Replace(Path.DirectorySeparatorChar, '.');
                sb.AppendFormat("    load(\"{0}\")", module);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("    System.init{");
            foreach(var type in types) {
                string name = ToCorrectTypeName(type.Key);
                sb.AppendFormat("        \"{0}\",", name);
                sb.AppendLine();
            }
            sb.AppendLine("    }");
            sb.Append("end");

            string manifestPath = Path.Combine(output_, "manifest.lua");
            File.WriteAllText(manifestPath, sb.ToString(), UTF8Encoding);
        }
    }
}
