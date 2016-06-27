using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using System.Runtime.CompilerServices;

namespace Bridge.Lua {
    public static class Wrap {
        private const string kWrapDllName = "__wrap__.dll";

        private static bool IsEnableType(TypeInfo type) {
            string name = type.Name;
            if(name[0] == '<' && name[name.Length - 1] == '>') {
                return false;
            }
            return true;
        }

        public static string Build(IEnumerable<Assembly> assemblys, string outDirectory, string bridgeDllPath) {
            List<CodeCompileUnit> units = new List<CodeCompileUnit>();
            foreach(Assembly assemblyDefinition in assemblys) {
                CodeCompileUnit unit = new CodeCompileUnit();
                Dictionary<string, CodeNamespace> namespaces = new Dictionary<string, CodeNamespace>();
                foreach(TypeInfo type in assemblyDefinition.DefinedTypes) {
                    if(IsEnableType(type)) {
                        CodeTypeDeclaration codeTypeDeclaration = TypeDefinitionBuilder.Build(type);
                        CodeNamespace codeNamespace = namespaces.GetOrDefault(type.Namespace);
                        if(codeNamespace ==  null) {
                            codeNamespace = new CodeNamespace(type.Namespace);
                            namespaces.Add(codeNamespace.Name, codeNamespace);
                        }
                        codeNamespace.Types.Add(codeTypeDeclaration);
                    }
                }
                unit.Namespaces.AddRange(namespaces.Values.ToArray());
                units.Add(unit);
            }

            CompilerParameters cp = new CompilerParameters();
            cp.CoreAssemblyFileName = bridgeDllPath;
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = false;
            cp.TreatWarningsAsErrors = true;
            cp.TempFiles.KeepFiles = true;
            cp.OutputAssembly = Path.Combine(outDirectory, kWrapDllName);
            cp.ReferencedAssemblies.Add(bridgeDllPath);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults cr = provider.CompileAssemblyFromDom(cp, units.ToArray());
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
    }

    public static class TypeDefinitionBuilder {
        public static CodeTypeDeclaration Build(TypeInfo type) {
            return GetCodeTypeDeclaration(type);
        }

        private static string GetTypeName(TypeInfo typeDefinition) {
            if(typeDefinition.IsGenericTypeDefinition) {
                int pos = typeDefinition.Name.IndexOf('`');
                return typeDefinition.Name.Substring(0, pos);
            }
            return typeDefinition.Name;
        }

        private static HashSet<string> filterExtendInterfaces_ = new HashSet<string>() {
            "System.Runtime.InteropServices._Attribute"
        };

        private static bool IsEnableExtendInterface(Type t) {
            if(filterExtendInterfaces_.Contains(t.FullName)) {
                return false;
            }
            return true;
        }

        private static bool IsEnableField(FieldInfo fieldInfo) {
            if(fieldInfo.IsPrivate) {
                return false;
            }

            if(fieldInfo.DeclaringType.IsEnum) {
                if(fieldInfo.Name == "value__") {
                    return false;
                }
            }
            return true;
        }

        private static bool IsEnableMethod(MethodInfo methodInfo) {
            if(methodInfo.IsPrivate) {
                return false;
            }

            if(methodInfo.CustomAttributes.Any(i => i.AttributeType == typeof(CompilerGeneratedAttribute))) {
                return false;
            }
            return true;
        }

        private static CodeTypeDeclaration GetCodeTypeDeclaration(TypeInfo typeDefinition) {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(GetTypeName(typeDefinition));
            if(typeDefinition.IsEnum) {
                declaration.IsEnum = true;
            }
            else if(typeDefinition.IsValueType) {
                declaration.IsStruct = true;
            }
            else if(typeDefinition.IsInterface) {
                declaration.IsInterface = true;
            }
            else if(typeDefinition.IsClass) {
                declaration.IsClass = true;
            }
            declaration.TypeAttributes = GetTypeAttributes(typeDefinition);
            if(declaration.IsClass) {
                if(typeDefinition.BaseType != null && typeDefinition.BaseType.FullName != "System.Object") {
                    declaration.BaseTypes.Add(typeDefinition.BaseType.FullName);
                }
            }
            if(declaration.IsClass || declaration.IsStruct) {
                foreach(Type i in typeDefinition.GetInterfaces()) {
                    if(IsEnableExtendInterface(i)) {
                        declaration.BaseTypes.Add(i.FullName);
                    }
                }
            }

            FillCustomAttribute(declaration.CustomAttributes, typeDefinition.CustomAttributes);
            FillGenericParameters(declaration.TypeParameters, typeDefinition.GenericTypeParameters);
            foreach(TypeInfo nestedType in typeDefinition.DeclaredNestedTypes) {
                declaration.Members.Add(GetCodeTypeDeclaration(nestedType));
            }
            foreach(ConstructorInfo constructorInfo in typeDefinition.DeclaredConstructors.Where(i => !i.IsPrivate)) {
                declaration.Members.Add(GetCodeMemberMethod(constructorInfo));
            }
            foreach(MethodInfo methodDefinition in typeDefinition.DeclaredMethods.Where(IsEnableMethod)) {
                declaration.Members.Add(GetCodeMemberMethod(methodDefinition));
            }
            foreach(FieldInfo fieldDefinition in typeDefinition.DeclaredFields.Where(IsEnableField)) {
                declaration.Members.Add(GetCodeMemberField(fieldDefinition));
            }
            foreach(PropertyInfo propertyDefinition in typeDefinition.DeclaredProperties.Where(i => i.GetGetMethod() != null)) {
                CodeMemberProperty codeMemberProperty = GetCodeMemberProperty(propertyDefinition);
                if(codeMemberProperty != null) {
                    declaration.Members.Add(codeMemberProperty);
                }
            }
            return declaration;
        }

        private static TypeAttributes GetTypeAttributes(TypeInfo typeDefinition) {
            TypeAttributes typeAttributes = 0;
            if(typeDefinition.IsAbstract) {
                typeAttributes |= TypeAttributes.Abstract;
            }
            if(typeDefinition.IsAnsiClass) {
                typeAttributes |= TypeAttributes.AnsiClass;
            }
            if(typeDefinition.IsAutoClass) {
                typeAttributes |= TypeAttributes.AutoClass;
            }
            if(typeDefinition.IsAutoLayout) {
                typeAttributes |= TypeAttributes.AutoLayout;
            }
            if(typeDefinition.IsClass) {
                typeAttributes |= TypeAttributes.Class;
            }
            if(typeDefinition.IsExplicitLayout) {
                typeAttributes |= TypeAttributes.ExplicitLayout;
            }
            if(typeDefinition.IsImport) {
                typeAttributes |= TypeAttributes.Import;
            }
            if(typeDefinition.IsInterface) {
                typeAttributes |= TypeAttributes.Interface;
            }
            if(typeDefinition.IsNestedAssembly) {
                typeAttributes |= TypeAttributes.NestedAssembly;
            }
            if(typeDefinition.IsNestedFamily) {
                typeAttributes |= TypeAttributes.NestedFamily;
            }
            if(typeDefinition.IsNestedPrivate) {
                typeAttributes |= TypeAttributes.NestedPrivate;
            }
            if(typeDefinition.IsNestedPublic) {
                typeAttributes |= TypeAttributes.NestedPublic;
            }
            if(typeDefinition.IsNotPublic) {
                typeAttributes |= TypeAttributes.NotPublic;
            }
            if(typeDefinition.IsPublic) {
                typeAttributes |= TypeAttributes.Public;
            }
            if(typeDefinition.IsSealed) {
                typeAttributes |= TypeAttributes.Sealed;
            }
            if(typeDefinition.IsSerializable) {
                typeAttributes |= TypeAttributes.Serializable;
            }
            if(typeDefinition.IsSpecialName) {
                typeAttributes |= TypeAttributes.SpecialName;
            }
            if(typeDefinition.IsUnicodeClass) {
                typeAttributes |= TypeAttributes.UnicodeClass;
            }
            return typeAttributes;
        }

        private static string GetParameterTypeName(Type typeReference) {
            string name = typeReference.FullName;
            if(typeReference.IsByRef) {
                name = name.Remove(name.Length - 1, 1);
            }
            return name;
        }

        private static CodeExpression GetDefalutValue(Type typeReference) {
            if(!typeReference.IsValueType) {
                return new CodePrimitiveExpression(null);
            }
            return new CodeDefaultValueExpression(new CodeTypeReference(GetParameterTypeName(typeReference)));
        }

        private static CodeMemberMethod GetCodeMemberMethod(MethodBase methodDefinition) {
            CodeMemberMethod codeMemberMethod;
            MethodInfo methodInfo = null;
            if(!methodDefinition.IsConstructor) {
                codeMemberMethod = new CodeMemberMethod() { Name = methodDefinition.Name };
                methodInfo = (MethodInfo)methodDefinition;
            }
            else {
                codeMemberMethod = new CodeConstructor();
            }

            codeMemberMethod.Attributes = GetMemberAttributes(methodDefinition);
            FillCustomAttribute(codeMemberMethod.CustomAttributes, methodDefinition.CustomAttributes);
            if(methodInfo != null) {
                FillGenericParameters(codeMemberMethod.TypeParameters, methodDefinition.GetGenericArguments());
            }
            if(methodInfo != null) {
                codeMemberMethod.ReturnType = new CodeTypeReference(methodInfo.ReturnType.FullName);
            }
            List<ParameterInfo> outParameters = new List<ParameterInfo>();
            foreach(ParameterInfo parameter in methodDefinition.GetParameters()) {
                CodeParameterDeclarationExpression p = new CodeParameterDeclarationExpression(GetParameterTypeName(parameter.ParameterType), parameter.Name);
                FillCustomAttribute(p.CustomAttributes, parameter.CustomAttributes);
                if(parameter.IsOut) {
                    p.Direction = FieldDirection.Out;
                    outParameters.Add(parameter);
                }
                else if(parameter.ParameterType.IsByRef) {
                    p.Direction = FieldDirection.Ref;
                }
                else if(parameter.IsIn) {
                    p.Direction = FieldDirection.In;
                }
                codeMemberMethod.Parameters.Add(p);
            }
            if(methodInfo != null) {
                if(!methodDefinition.IsAbstract) {
                    foreach(ParameterInfo outParameter in outParameters) {
                        CodeAssignStatement assign = new CodeAssignStatement(
                            new CodeArgumentReferenceExpression(outParameter.Name),
                            GetDefalutValue(outParameter.ParameterType));
                        codeMemberMethod.Statements.Add(assign);
                    }
                    if(methodInfo.ReturnType.FullName != "System.Void") {
                        codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(GetDefalutValue(methodInfo.ReturnType)));
                    }
                }
            }
            return codeMemberMethod;
        }

        private static MemberAttributes GetMemberAttributes(MethodBase methodDefinition) {
            MemberAttributes memberAttributes = 0;
            if(methodDefinition.IsAbstract) {
                memberAttributes |= MemberAttributes.Abstract;
            }
            if(!methodDefinition.IsVirtual) {
                memberAttributes |= MemberAttributes.Final;
            }
            if(methodDefinition.IsStatic) {
                memberAttributes |= MemberAttributes.Static;
            }
            if(methodDefinition.IsAssembly) {
                memberAttributes |= MemberAttributes.Assembly;
            }
            if(methodDefinition.IsFamilyAndAssembly) {
                memberAttributes |= MemberAttributes.FamilyAndAssembly;
            }
            if(methodDefinition.IsFamily) {
                memberAttributes |= MemberAttributes.Family;
            }
            if(methodDefinition.IsFamilyOrAssembly) {
                memberAttributes |= MemberAttributes.FamilyOrAssembly;
            }
            if(methodDefinition.IsPrivate) {
                memberAttributes |= MemberAttributes.Private;
            }
            if(methodDefinition.IsPublic) {
                memberAttributes |= MemberAttributes.Public;
            }
            return memberAttributes;
        }

        private static void FillGenericParameters(CodeTypeParameterCollection codes, IEnumerable<Type> defines) {
            foreach(Type genericParameter in defines) {
                CodeTypeParameter t = new CodeTypeParameter(genericParameter.FullName ?? genericParameter.Name);
                if(genericParameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)) {
                    t.HasConstructorConstraint = true;
                }
                foreach(var constraint in genericParameter.GetGenericParameterConstraints()) {
                    t.Constraints.Add(new CodeTypeReference(constraint.FullName));
                }
                FillCustomAttribute(t.CustomAttributes, genericParameter.CustomAttributes);
                codes.Add(t);
            }
        }

        private static void FillCustomAttribute(CodeAttributeDeclarationCollection codes, IEnumerable<CustomAttributeData> defines) {
            foreach(CustomAttributeData customAttribute in defines) {
                List<CodeAttributeArgument> codeAttributeArguments = new List<CodeAttributeArgument>();
                foreach(CustomAttributeTypedArgument attributeArgument in customAttribute.ConstructorArguments) {
                    CodeExpression codeExpress;
                    if(attributeArgument.Value.GetType().FullName == attributeArgument.ArgumentType.FullName) {
                        codeExpress = new CodePrimitiveExpression(attributeArgument.Value);
                    }
                    else {
                        codeExpress = new CodeCastExpression(attributeArgument.ArgumentType.FullName, new CodePrimitiveExpression(attributeArgument.Value));
                    }
                    codeAttributeArguments.Add(new CodeAttributeArgument(codeExpress));
                }
                foreach(CustomAttributeNamedArgument attributeNamedArgument in customAttribute.NamedArguments) {
                    codeAttributeArguments.Add(new CodeAttributeArgument(attributeNamedArgument.MemberName, new CodePrimitiveExpression(attributeNamedArgument.TypedValue.Value)));
                }
                CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(customAttribute.AttributeType.FullName, codeAttributeArguments.ToArray());
                codes.Add(codeAttributeDeclaration);
            }
        }

        private static MemberAttributes GetFieldAttribute(FieldInfo fieldDefinition) {
            MemberAttributes memberAttributes = 0;
            if(fieldDefinition.IsStatic) {
                memberAttributes |= MemberAttributes.Static;
            }
            /*
            if(fieldDefinition.HasConstant) {
                memberAttributes |= MemberAttributes.Const;
            }*/
            if(fieldDefinition.IsAssembly) {
                memberAttributes |= MemberAttributes.Assembly;
            }
            if(fieldDefinition.IsFamilyAndAssembly) {
                memberAttributes |= MemberAttributes.FamilyAndAssembly;
            }
            if(fieldDefinition.IsFamily) {
                memberAttributes |= MemberAttributes.Family;
            }
            if(fieldDefinition.IsFamilyOrAssembly) {
                memberAttributes |= MemberAttributes.FamilyOrAssembly;
            }
            if(fieldDefinition.IsPrivate) {
                memberAttributes |= MemberAttributes.Private;
            }
            if(fieldDefinition.IsPublic) {
                memberAttributes |= MemberAttributes.Public;
            }
            return memberAttributes;
        }

        private static CodeMemberField GetCodeMemberField(FieldInfo fieldDefinition) {
            CodeMemberField codeMemberField = new CodeMemberField(fieldDefinition.FieldType.FullName, fieldDefinition.Name);
            codeMemberField.Attributes = GetFieldAttribute(fieldDefinition);
            /*
            if(fieldDefinition.Constant != null) {
                codeMemberField.InitExpression = new CodePrimitiveExpression(fieldDefinition.Constant);
            }*/
            FillCustomAttribute(codeMemberField.CustomAttributes, fieldDefinition.CustomAttributes);
            return codeMemberField;
        }

        private static CodeMemberProperty GetCodeMemberProperty(PropertyInfo propertyDefinition) {
            CodeMemberProperty codeMemberProperty = new CodeMemberProperty() {
                Type = new CodeTypeReference(propertyDefinition.PropertyType.FullName),
                Name = propertyDefinition.Name,
                Attributes = GetMemberAttributes(propertyDefinition.GetGetMethod()),
            };
            FillCustomAttribute(codeMemberProperty.CustomAttributes, propertyDefinition.CustomAttributes);
            if(propertyDefinition.GetMethod != null) {
                codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(GetDefalutValue(propertyDefinition.PropertyType)));
            }
            if(propertyDefinition.SetMethod != null) {
                CodeAssignStatement assign = new CodeAssignStatement(
                    new CodeArgumentReferenceExpression("value"),
                    GetDefalutValue(propertyDefinition.PropertyType));
                codeMemberProperty.SetStatements.Add(assign);
            }
            return codeMemberProperty;
        }
    }
}
