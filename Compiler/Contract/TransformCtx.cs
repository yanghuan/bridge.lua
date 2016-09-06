using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ICSharpCode.NRefactory.TypeSystem;
using System.Xml.Serialization;
using Mono.Cecil;
using ICSharpCode.NRefactory.TypeSystem.Implementation;

namespace Bridge.Contract {
    public sealed class RawString {
        public string s_;

        public RawString(string s) {
            s_ = s;
        }

        public override string ToString() {
            return s_;
        }
    }

    public static class TransformCtx {
        public const string DefaultString = "__default__";
        public const string DefaultInvoke = DefaultString + "()";

        public static T GetOrDefault<K, T>(this IDictionary<K, T> dict, K key, T t = default(T)) {
            T v;
            if(dict.TryGetValue(key, out v)) {
                return v;
            }
            return t;
        }

        public sealed class MethodInfo {
            public string Name;
            public bool IsPrivate;
            public bool IsCtor;
        }

        public static readonly HashSet<string> CurUsingNamespaces = new HashSet<string>();
        public static Func<IEntity, string> GetEntityName;
        public static List<MethodInfo> CurClassMethodNames = new List<MethodInfo>();
        public static IType CurClass;
        public static List<string> CurClassOtherMethods = new List<string>();
        public static List<MethodInfo> CurClassOtherMethodNames = new List<MethodInfo>();
        public static Dictionary<ITypeInfo, string> NamespaceNames = new Dictionary<ITypeInfo, string>();
        public static HashSet<IType> ExportEnums = new HashSet<IType>();
    }

    [XmlRoot("assembly")]
    public sealed class XmlMetaModel {
        public sealed class TemplateModel {
            [XmlAttribute]
            public string Template;
        }

        public sealed class PropertyModel {
            [XmlAttribute]
            public string name;
            [XmlAttribute]
            public string Name;
            [XmlElement]
            public TemplateModel set;
            [XmlElement]
            public TemplateModel get;
        }

        public sealed class ArgumentModel {
            [XmlAttribute]
            public string type;
        }

        public sealed class MethodModel {
            [XmlAttribute]
            public string name;
            [XmlAttribute]
            public string Name;
            [XmlAttribute]
            public string Template;
            [XmlElement("arg")]
            public ArgumentModel[] Args;

            public bool IsMatchAll {
                get {
                    return Args == null;
                }
            }
        }

        public sealed class ClassModel {
            [XmlAttribute]
            public string name;
            [XmlAttribute]
            public string Name;
            [XmlElement("property")]
            public PropertyModel[] Propertys;
            [XmlElement("method")]
            public MethodModel[] Methods;
        }

        public sealed class NamespaceModel {
            [XmlAttribute]
            public string name;
            [XmlAttribute]
            public string Name;
            [XmlElement("class")]
            public ClassModel[] Classes;
        }

        [XmlElement("namespace")]
        public NamespaceModel[] Namespaces;
    }

    public sealed class XmlMetaMaker {
        public sealed class TypeMetaInfo {
            private XmlMetaModel.ClassModel model_;
            public TypeDefinition TypeDefinition { get; private set; }

            public TypeMetaInfo(TypeDefinition typeDefinition, XmlMetaModel.ClassModel model) {
                TypeDefinition = typeDefinition;
                model_ = model;
                Property();
                Method();
            }

            public string Name {
                get {
                    return model_.Name;
                }
            }

            private void Property() {
                if(model_.Propertys != null) {
                    foreach(var propertyModel in model_.Propertys) {
                        PropertyDefinition propertyDefinition = TypeDefinition.Properties.FirstOrDefault(i => i.Name == propertyModel.name);
                        if(propertyDefinition == null) {
                            throw new ArgumentException(propertyModel.name + " is not found at " + TypeDefinition.FullName);
                        }
                        PropertyMataInfo info = new PropertyMataInfo(propertyDefinition, propertyModel);
                        XmlMetaMaker.AddProperty(info);
                    }
                }
            }

            private bool IsMethodParameterSame(TypeReference typeReference, XmlMetaModel.ArgumentModel argument) {
                if(typeReference.IsGenericParameter) {
                    return typeReference.Name == argument.type;
                }
                else if(typeReference.IsGenericInstance) {
                    throw new NotSupportedException();
                }
                else if(typeReference.IsNested) {
                    return typeReference.FullName.Replace("/", ".") == argument.type;
                }
                else {
                    return typeReference.FullName == argument.type;
                }
            }

            private bool IsMethodMatch(MethodDefinition methodDefinition, XmlMetaModel.MethodModel model) {
                if(methodDefinition.Name != model.name) {
                    return false;
                }

                if(methodDefinition.Parameters.Count != model.Args.Length) {
                    return false;
                }

                int index = 0;
                foreach(var parameter in methodDefinition.Parameters) {
                    var parameterModel = model.Args[index];
                    if(!IsMethodParameterSame(parameter.ParameterType, parameterModel)) {
                        return false;
                    }
                    ++index;
                }

                return true;
            }

            private void Method() {
                if(model_.Methods != null) {
                    foreach(var methodModel in model_.Methods) {
                        if(methodModel.IsMatchAll) {
                            var methods = TypeDefinition.Methods.Where(i => i.Name == methodModel.name);
                            foreach(MethodDefinition methodDefinition in methods) {
                                MethodMateInfo info = new MethodMateInfo(methodDefinition, methodModel);
                                XmlMetaMaker.AddMethod(info);
                            }
                        }
                        else {
                            MethodDefinition methodDefinition = TypeDefinition.Methods.FirstOrDefault(i => IsMethodMatch(i, methodModel));
                            if(methodDefinition == null) {
                                throw new ArgumentException(methodModel + " is not found match");
                            }
                            MethodMateInfo info = new MethodMateInfo(methodDefinition, methodModel);
                            XmlMetaMaker.AddMethod(info);
                        }
                    }
                }
            }
        }

        public sealed class PropertyMataInfo {
            private XmlMetaModel.PropertyModel model_;
            public PropertyDefinition PropertyDefinition { get; private set; }

            public PropertyMataInfo(PropertyDefinition propertyDefinition, XmlMetaModel.PropertyModel model) {
                PropertyDefinition = propertyDefinition;
                model_ = model;
            }

            public string GetTemplate(bool isGet) {
                var model = isGet ? model_.get : model_.set;
                return model.Template;
            }

            public string Name {
                get {
                    return model_.Name;
                }
            }
        }

        public sealed class MethodMateInfo {
            private XmlMetaModel.MethodModel model_;
            public MethodDefinition MethodDefinition { get; private set; }

            public MethodMateInfo(MethodDefinition methodDefinition, XmlMetaModel.MethodModel model) {
                MethodDefinition = methodDefinition;
                model_ = model;
            }

            public string Template {
                get {
                    return model_.Template;
                }
            }

            public string Name {
                get {
                    return model_.Name;
                }
            }
        }

        private static IEmitter emitter_;
        private static Dictionary<string, string> namespaceMaps_ = new Dictionary<string, string>();
        private static Dictionary<TypeDefinition, TypeMetaInfo> types_ = new Dictionary<TypeDefinition, TypeMetaInfo>();
        private static Dictionary<PropertyDefinition, PropertyMataInfo> propertys_ = new Dictionary<PropertyDefinition, PropertyMataInfo>();
        private static Dictionary<MethodDefinition, MethodMateInfo> methods_ = new Dictionary<MethodDefinition, MethodMateInfo>();

        public static void Load(IEnumerable<string> files, IEmitter emitter) {
            emitter_ = emitter;

            foreach(string file in files) {
                XmlSerializer xmlSeliz = new XmlSerializer(typeof(XmlMetaModel));
                try {
                    using(Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        XmlMetaModel model = (XmlMetaModel)xmlSeliz.Deserialize(stream);
                        if(model.Namespaces != null) {
                            foreach(var namespaceModel in model.Namespaces) {
                                Load(namespaceModel);
                            }
                        }
                    }
                }
                catch(Exception e) {
                    throw new Exception("load xml file wrong at " + file, e);
                }
            }
        }

        private static void FixName(ref string name) {
            name = name.Replace('^', '`');
        }

        private static void Load(XmlMetaModel.NamespaceModel model) {
            string namespaceName = model.name;
            if(string.IsNullOrEmpty(namespaceName)) {
                throw new ArgumentException("namespace's name is empty");
            }

            if(!string.IsNullOrEmpty(model.Name)) {
                if(namespaceMaps_.ContainsKey(namespaceName)) {
                    throw new ArgumentException(namespaceName + " namespace map is already has");
                }
                namespaceMaps_.Add(namespaceName, model.Name);
            }

            if(emitter_.BridgeTypes.IsNamespaceExists(model.name)) {
                if(model.Classes != null) {
                    foreach(var classModel in model.Classes) {
                        string className = classModel.name;
                        if(string.IsNullOrEmpty(className)) {
                            throw new ArgumentException(string.Format("namespace[{0}] has a class's name is empty", namespaceName));
                        }

                        string keyName = namespaceName + '.' + className;
                        FixName(ref keyName);
                        var type = emitter_.BridgeTypes.GetOrDefault(keyName);
                        if(type != null) {
                            if(types_.ContainsKey(type.TypeDefinition)) {
                                throw new ArgumentException(type.TypeDefinition.FullName + " is already has");
                            }

                            TypeMetaInfo info = new TypeMetaInfo(type.TypeDefinition, classModel);
                            types_.Add(info.TypeDefinition, info);
                        }
                        else {
                            emitter_.Log.Warn(keyName + " is not found at BridgeTypes");
                        }
                    }
                }
            }
            else {
                emitter_.Log.Info("namespace " + model.name + " is not load meta data xml");
            }
        }

        public static string GetCustomName(TypeDefinition type) {
            var info = types_.GetOrDefault(type);
            return info != null ? info.Name : null;
        }

        private static void AddProperty(PropertyMataInfo info) {
            if(propertys_.ContainsKey(info.PropertyDefinition)) {
                throw new ArgumentException(info.PropertyDefinition.FullName + " is already has");
            }
            propertys_.Add(info.PropertyDefinition, info);
        }

        public static string GetPropertyName(IProperty property) {
            if(!property.IsPublic) {
                return null;
            }
            var type = emitter_.BridgeTypes.Get(property.MemberDefinition.DeclaringType);
            PropertyDefinition propertyDefinition = type.TypeDefinition.Properties.First(i => i.Name == property.Name);
            var info = propertys_.GetOrDefault(propertyDefinition);
            return info != null ? info.Name : null;
        }

        public static string GetPropertyInline(IProperty property, bool isGet) {
            if(!property.IsPublic) {
                return null;
            }
            var type = emitter_.BridgeTypes.Get(property.MemberDefinition.DeclaringType);
            PropertyDefinition propertyDefinition = type.TypeDefinition.Properties.First(i => i.Name == property.Name);
            var info = propertys_.GetOrDefault(propertyDefinition);
            return info != null ? info.GetTemplate(isGet) : null;
        }

        public static string GetNamespace(string name) {
            return namespaceMaps_.GetOrDefault(name, name);
        }

        private static void AddMethod(MethodMateInfo info) {
            if(methods_.ContainsKey(info.MethodDefinition)) {
                throw new ArgumentException(info.MethodDefinition.FullName + " is already has");
            }
            methods_.Add(info.MethodDefinition, info);
        }

        public static string ToCorrectTypeName(string keyName) {
            return BridgeTypes.ConvertName(keyName);
        }

        private static bool IsSame(TypeReference typeReference, IType type) {
            if(typeReference.IsGenericParameter) {
                if(type.Kind == TypeKind.TypeParameter) {
                    return typeReference.Name == type.Name;
                }
            }
            else if(typeReference.IsGenericInstance) {
                GenericInstanceType genericInstanceType = (GenericInstanceType)typeReference;
                if(genericInstanceType.GenericArguments.Count == type.TypeParameterCount) {
                    int index = 0;
                    foreach(var genericArgument in genericInstanceType.GenericArguments) {
                        var typeArgument = type.TypeArguments[index];
                        if(!IsSame(genericArgument, typeArgument)) {
                            return false;
                        }
                        ++index;
                    }
                    return true;
                }
                return false;
            }
            else if(typeReference.IsNested) {
                return typeReference.FullName.Replace("/", ".") == type.FullName;
            }
            else {
                return typeReference.FullName == type.FullName;
            }
            return false;
        }

        private static bool IsMatch(MethodDefinition methodDefinition, IMethod method) {
            if(methodDefinition.Name != method.Name) {
                return false;
            }

            if(methodDefinition.Parameters.Count != method.Parameters.Count) {
                return false;
            }

            int index = 0;
            foreach(var parameterDefinition in methodDefinition.Parameters) {
                var parameter = method.Parameters[index];
                if(parameter.Name != parameterDefinition.Name) {
                    return false;
                }
                if(!IsSame(parameterDefinition.ParameterType, parameter.Type)) {
                    return false;
                }
                ++index;
            }
            return true;
        }

        private static Dictionary<IMethod, MethodDefinition> methodDefinitionMaps_ = new Dictionary<IMethod, MethodDefinition>();

        private static MethodDefinition GetMethodDefinition(IMethod method) {
            method = (IMethod)method.MemberDefinition;

            MethodDefinition methodDefinition;
            if(!methodDefinitionMaps_.TryGetValue(method, out methodDefinition)) {
                var type = emitter_.BridgeTypes.Get(method.DeclaringType);
                methodDefinition = type.TypeDefinition.Methods.FirstOrDefault(i => IsMatch(i, method));
                methodDefinitionMaps_.Add(method, methodDefinition);
            }

            return methodDefinition;
        }

        public static string GetMethodInline(IMethod method) {
            if(!method.IsPublic) {
                return null;
            }

            MethodDefinition methodDefinition = GetMethodDefinition(method);
            if(methodDefinition != null) {
                var info = methods_.GetOrDefault(methodDefinition);
                return info != null ? info.Template : null;
            }
            return null;
        }

        public static string GetMethodName(IMethod method) {
            if(!method.IsPublic) {
                return null;
            }

            MethodDefinition methodDefinition = GetMethodDefinition(method);
            if(methodDefinition != null) {
                var info = methods_.GetOrDefault(methodDefinition);
                return info != null ? info.Name : null;
            }
            return null;
        }
    }
}
