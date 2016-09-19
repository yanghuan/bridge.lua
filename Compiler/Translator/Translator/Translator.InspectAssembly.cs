using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Bridge.Translator
{
    public partial class Translator
    {
        private string GetAssemblyPath(string assemblyName) {
            if(SearchPaths.Count > 0) {
                foreach(string dir in SearchPaths) {
                    string path = Path.Combine(dir, assemblyName + ".dll");
                    if(File.Exists(path)) {
                        return path;
                    }
                }
            }
            else {
                string dir = RuntimeEnvironment.GetRuntimeDirectory();
                string path = Path.Combine(dir, assemblyName + ".dll");
                if(File.Exists(path)) {
                    return path;
                }
            }

            throw new System.Exception("not found assembly " + assemblyName);
        }

        protected virtual AssemblyDefinition LoadAssembly(string location, List<AssemblyDefinition> references)
        {
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(location);
            foreach(AssemblyNameReference r in assemblyDefinition.MainModule.AssemblyReferences) {
                string path = GetAssemblyPath(r.Name);
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path);
                references.Add(assembly);
            }
            return assemblyDefinition;

            /*
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(location);
            string name;
            string path;
            AssemblyDefinition reference;

            foreach (AssemblyNameReference r in assemblyDefinition.MainModule.AssemblyReferences)
            {
                name = r.Name;
                path = GetAssemblyPath(name);
                reference = this.LoadAssembly(path, references);
                if (!references.Any(a => a.Name.Name == reference.Name.Name))
                {
                    references.Add(reference);
                }
            }
            return assemblyDefinition;*/
        }

        protected virtual void ReadTypes(AssemblyDefinition assembly, bool isCodeAssembly)
        {
            this.AddNestedTypes(assembly.MainModule.Types, isCodeAssembly);
        }

        public bool IsEnableType(TypeDefinition type) {
            if(!type.IsPublic && !type.IsNestedPublic) {
                return false;
            }

            string name = type.Name;
            if(name == "<Module>") {
                return false;
            }

            if(type.CustomAttributes.Any(i => i.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute")) {
                return false;
            }

            return true;
        }

        protected virtual void AddNestedTypes(IEnumerable<TypeDefinition> types, bool isCodeAssembly)
        {
            foreach (TypeDefinition type in types)
            {
                if (type.FullName.Contains("<"))
                {
                    continue;
                }

                if(!isCodeAssembly) {
                    if(!IsEnableType(type)) {
                        continue;
                    }
                }

                string key = BridgeTypes.GetTypeDefinitionKey(type);
                this.Validator.CheckType(type, this);
                if(this.TypeDefinitions.ContainsKey(key)) {
                    throw new System.Exception(type + " is already exists");
                }

                this.TypeDefinitions.Add(key, type);
                this.BridgeTypes.Add(key, new BridgeType(key)
                {
                    TypeDefinition = type,
                    IsFromCode = isCodeAssembly,
                });

                if (type.HasNestedTypes)
                {
                    Translator.InheritAttributes(type);
                    this.AddNestedTypes(type.NestedTypes, isCodeAssembly);
                }
            }
        }

        /// <summary>
        /// Makes any existing nested types (classes?) inherit the FileName attribute of the specified type.
        /// Does not override a nested type's FileName attribute if present.
        /// </summary>
        /// <param name="type"></param>
        protected static void InheritAttributes(TypeDefinition type)
        {
            // List of attribute names that are meant to be inherited by sub-classes.
            var attrList = new List<string>
            {
                "FileNameAttribute",
                "ModuleAttribute",
                "NamespaceAttribute"
            };

            foreach (var attribute in attrList)
            {
                if (type.CustomAttributes.Any(ca => ca.AttributeType.Name == attribute))
                {
                    var FAt = type.CustomAttributes.First(ca => ca.AttributeType.Name == attribute);

                    foreach (var nestedType in type.NestedTypes)
                    {
                        if (!nestedType.CustomAttributes.Any(ca => ca.AttributeType.Name == attribute))
                        {
                            nestedType.CustomAttributes.Add(FAt);
                        }
                    }
                }
            }
        }

        protected virtual List<AssemblyDefinition> InspectReferences()
        {
            this.TypeInfoDefinitions = new Dictionary<string, ITypeInfo>();

            var references = new List<AssemblyDefinition>();
            var assembly = this.LoadAssembly(this.AssemblyLocation, references);
            this.TypeDefinitions = new Dictionary<string, TypeDefinition>();
            this.BridgeTypes = new BridgeTypes();
            this.AssemblyDefinition = assembly;

            this.ReadTypes(assembly, true);
            foreach (var item in references)
            {
                this.ReadTypes(item, false);
            }

            if (!this.FolderMode)
            {
                var prefix = Path.GetDirectoryName(this.Location);

                for (int i = 0; i < this.SourceFiles.Count; i++)
                {
                    this.SourceFiles[i] = Path.Combine(prefix, this.SourceFiles[i]);
                }
            }

            return references;
        }

        protected virtual void InspectTypes(MemberResolver resolver, IAssemblyInfo config)
        {
            Inspector inspector = this.CreateInspector();
            inspector.AssemblyInfo = config;
            inspector.Resolver = resolver;

            for (int i = 0; i < this.ParsedSourceFiles.Count; i++)
            {
                inspector.VisitSyntaxTree(this.ParsedSourceFiles[i].SyntaxTree);
            }

            this.AssemblyInfo = inspector.AssemblyInfo;
            this.Types = inspector.Types;
        }

        protected virtual Inspector CreateInspector()
        {
            return new Inspector();
        }

        protected void BuildSyntaxTree()
        {
            for (int i = 0; i < this.SourceFiles.Count; i++)
            {
                var fileName = this.SourceFiles[i];

                using (var reader = new StreamReader(fileName))
                {
                    var parser = new ICSharpCode.NRefactory.CSharp.CSharpParser();

                    if (this.DefineConstants != null && this.DefineConstants.Count > 0)
                    {
                        foreach (var defineConstant in this.DefineConstants)
                        {
                            parser.CompilerSettings.ConditionalSymbols.Add(defineConstant);
                        }
                    }

                    var syntaxTree = parser.Parse(reader, fileName);

                    if (parser.HasErrors)
                    {
                        foreach (var error in parser.Errors)
                        {
                            throw new EmitterException(syntaxTree, string.Format("Parsing error in a file {0} {2}: {1}", fileName, error.Message, error.Region.Begin.ToString()));
                        }
                    }

                    var expandResult = new QueryExpressionExpander().ExpandQueryExpressions(syntaxTree);
                    syntaxTree = (expandResult != null ? (SyntaxTree)expandResult.AstNode : syntaxTree);

                    var emptyLambdaDetecter = new EmptyLambdaDetecter();
                    syntaxTree.AcceptVisitor(emptyLambdaDetecter);

                    if (emptyLambdaDetecter.Found)
                    {
                        var fixer = new EmptyLambdaFixer();
                        var astNode = syntaxTree.AcceptVisitor(fixer);
                        syntaxTree = (astNode != null ? (SyntaxTree)astNode : syntaxTree);
                    }

                    var f = new ParsedSourceFile(syntaxTree, new CSharpUnresolvedFile
                    {
                        FileName = fileName
                    });
                    this.ParsedSourceFiles.Add(f);

                    var tcv = new TypeSystemConvertVisitor(f.ParsedFile);
                    f.SyntaxTree.AcceptVisitor(tcv);
                }
            }
        }
    }
}
