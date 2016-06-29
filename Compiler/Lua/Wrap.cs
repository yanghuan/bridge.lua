using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mono.Cecil;

using TypeAttributes = System.Reflection.TypeAttributes;

namespace Bridge.Lua {
    public static class Wrap {
        private const string kWrapDllName = "__wrap__.dll";

        public static string Build(IEnumerable<AssemblyDefinition> assemblys, string outDirectory, string bridgeDllPath, string libWhite, string libBlack) {
            TypeDefinitionBuilder builder = new TypeDefinitionBuilder(libWhite, libBlack);
            List<CodeCompileUnit> units = new List<CodeCompileUnit>();

            foreach(var assemblyDefinition in assemblys) {
                CodeCompileUnit unit = new CodeCompileUnit();
                Dictionary<string, CodeNamespace> namespaces = new Dictionary<string, CodeNamespace>();
                foreach(var module in assemblyDefinition.Modules) {
                    foreach(var type in module.Types) {
                        if(builder.IsEnableType(type)) {
                            CodeTypeDeclaration codeTypeDeclaration = builder.Build(type);
                            CodeNamespace codeNamespace = namespaces.GetOrDefault(type.Namespace);
                            if(codeNamespace == null) {
                                codeNamespace = new CodeNamespace(type.Namespace);
                                namespaces.Add(codeNamespace.Name, codeNamespace);
                            }
                            codeNamespace.Types.Add(codeTypeDeclaration);
                        }
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
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, units.Select(i => i.Compile()).ToArray());
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

    public sealed class TypeDefinitionBuilder {
        private sealed class FilterInfo {
            private HashSet<string> namespaces_ = new HashSet<string>();
            private HashSet<string> types_ = new HashSet<string>();

            public FilterInfo(string namesString) {
                if(!string.IsNullOrWhiteSpace(namesString)) {
                    string[] names = namesString.Split(';');
                    foreach(string name in names) {
                        if(name.StartsWith("#")) {
                            namespaces_.Add(name.Remove(0, 1));
                        }
                        else {
                            types_.Add(name);
                        }
                    }
                }
            }

            public bool IsEmpty {
                get {
                    return namespaces_.Count == 0 && types_.Count == 0;
                }
            }

            public bool IsExists(TypeDefinition type) {
                if(namespaces_.Contains(type.Namespace)) {
                    return true;
                }

                if(namespaces_.Contains(GetName(type))) {
                    return true;
                }

                return false;
            }

            private string GetName(TypeDefinition typeDefinition) {
                string name = typeDefinition.FullName;
                if(typeDefinition.HasGenericParameters) {
                    name = Utility.RemoveGenericInstanceSign(name);
                }
                if(typeDefinition.IsNested) {
                    name = name.Replace('/', '.');
                }
                return name;
            }
        }

        private FilterInfo whiteFilter_;
        private FilterInfo blackFilter_;

        public TypeDefinitionBuilder(string libWhite, string libBlack) {
            whiteFilter_ = new FilterInfo(libWhite);
            blackFilter_ = new FilterInfo(libBlack);
        }

        public CodeTypeDeclaration Build(TypeDefinition type) {
            return GetCodeTypeDeclaration(type);
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

            if(!whiteFilter_.IsEmpty && !whiteFilter_.IsExists(type)) {
                return false;
            }

            if(blackFilter_.IsExists(type)) {
                return false;
            }

            return true;
        }

        private static string GetTypeName(TypeDefinition typeDefinition) {
            if(typeDefinition.HasGenericParameters) {
                return Utility.RemoveGenericInstanceSign(typeDefinition.Name);
            }
            return typeDefinition.Name;
        }

        private static bool IsEnableField(FieldDefinition fieldInfo) {
            if(!fieldInfo.IsPublic) {
                return false;
            }

            if(fieldInfo.DeclaringType.IsEnum) {
                if(fieldInfo.Name == "value__") {
                    return false;
                }
            }
            return true;
        }

        private static bool IsEnableMethod(MethodDefinition methodInfo) {
            if(!methodInfo.IsPublic) {
                return false;
            }

            if(methodInfo.IsGetter || methodInfo.IsSetter) {
                return false;
            }
            return true;
        }

        private CodeTypeDeclaration GetCodeTypeDelegate(TypeDefinition typeDefinition) {
            if(typeDefinition.IsClass && typeDefinition.BaseType.FullName == "System.MulticastDelegate") {
                CodeTypeDelegate codeTypeDelegate = new CodeTypeDelegate() { Name = GetTypeName(typeDefinition) };
                if(typeDefinition.IsPublic || typeDefinition.IsNestedPublic) {
                    codeTypeDelegate.TypeAttributes = TypeAttributes.Public;
                }
                FillCustomAttribute(codeTypeDelegate.CustomAttributes, typeDefinition.CustomAttributes);
                FillGenericParameters(codeTypeDelegate.TypeParameters, typeDefinition.GenericParameters);
                MethodDefinition methodDefinition = typeDefinition.Methods.First(i => i.Name == "Invoke");
                codeTypeDelegate.ReturnType = new CodeTypeReference(GetTypeReferenceName(methodDefinition.ReturnType));
                codeTypeDelegate.Parameters.AddRange(methodDefinition.Parameters.Select(GetParameterExpression).ToArray());
                if(typeDefinition.HasGenericParameters) {
                    string sign = string.Join(",", typeDefinition.GenericParameters.Select(i => i.FullName));
                    codeTypeDelegate.Name += '<' + sign + '>'; 
                }
                return codeTypeDelegate;
            }
            return null;
        }

        private CodeTypeDeclaration GetCodeTypeDeclaration(TypeDefinition typeDefinition) {
            var codeTypeDelegate = GetCodeTypeDelegate(typeDefinition);
            if(codeTypeDelegate != null) {
                return codeTypeDelegate;
            }

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
                if(typeDefinition.BaseType != null && typeDefinition.BaseType.MetadataType != MetadataType.Object) {
                    declaration.BaseTypes.Add(GetTypeReferenceName(typeDefinition.BaseType));
                }
            }
            else if(declaration.IsEnum) {
                FieldDefinition fieldDefinition = typeDefinition.Fields.First(i => i.Name == "value__");
                if(fieldDefinition.FieldType.MetadataType != MetadataType.Int32) {
                    declaration.BaseTypes.Add(GetTypeReferenceName(fieldDefinition.FieldType));
                }
            }

            if(declaration.IsClass || declaration.IsStruct) {
                foreach(var i in typeDefinition.Interfaces) {
                    declaration.BaseTypes.Add(GetTypeReferenceName(i));
                }
            }

            FillCustomAttribute(declaration.CustomAttributes, typeDefinition.CustomAttributes);
            FillGenericParameters(declaration.TypeParameters, typeDefinition.GenericParameters);
            foreach(var nestedType in typeDefinition.NestedTypes.Where(IsEnableType)) {
                declaration.Members.Add(GetCodeTypeDeclaration(nestedType));
            }
            foreach(var methodDefinition in typeDefinition.Methods.Where(IsEnableMethod)) {
                declaration.Members.Add(GetCodeMemberMethod(methodDefinition));
            }
            foreach(var fieldDefinition in typeDefinition.Fields.Where(IsEnableField)) {
                declaration.Members.Add(GetCodeMemberField(fieldDefinition));
            }
            foreach(var propertyDefinition in typeDefinition.Properties.Where(i => i.GetMethod != null && i.GetMethod.IsPublic)) {
                CodeMemberProperty codeMemberProperty = GetCodeMemberProperty(propertyDefinition);
                if(codeMemberProperty != null) {
                    declaration.Members.Add(codeMemberProperty);
                }
            }
            return declaration;
        }

        private static TypeAttributes GetTypeAttributes(TypeDefinition typeDefinition) {
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

        private static string GetTypeReferenceName(TypeReference typeReference) {
            string name = typeReference.FullName;
            if(typeReference.IsByReference) {
                name = name.Remove(name.Length - 1, 1);
            }
            if(typeReference.IsGenericInstance) {
                name = Utility.RemoveGenericInstanceSign(name);
            }
            if(typeReference.IsNested || typeReference.GetElementType().IsNested) {
                name = name.Replace('/', '.');
            }
            return name;
        }

        private static CodeExpression GetDefalutValue(TypeReference typeReference) {
            if(!typeReference.IsValueType) {
                return new CodePrimitiveExpression(null);
            }
            return new CodeDefaultValueExpression(new CodeTypeReference(GetTypeReferenceName(typeReference)));
        }

        private CodeParameterDeclarationExpression GetParameterExpression(ParameterDefinition parameter) {
            CodeParameterDeclarationExpression p = new CodeParameterDeclarationExpression(GetTypeReferenceName(parameter.ParameterType), parameter.Name);
            FillCustomAttribute(p.CustomAttributes, parameter.CustomAttributes);
            if(parameter.IsOut) {
                p.Direction = FieldDirection.Out;
            }
            else if(parameter.ParameterType.IsByReference) {
                p.Direction = FieldDirection.Ref;
            }
            else if(parameter.IsIn) {
                p.Direction = FieldDirection.In;
            }
            return p;
        }

        private CodeMemberMethod GetCodeMemberMethod(MethodDefinition methodDefinition) {
            CodeMemberMethod codeMemberMethod;
            if(!methodDefinition.IsConstructor) {
                codeMemberMethod = new CodeMemberMethod() { Name = methodDefinition.Name };
            }
            else {
                codeMemberMethod = new CodeConstructor();
            }

            codeMemberMethod.Attributes = GetMemberAttributes(methodDefinition);
            FillCustomAttribute(codeMemberMethod.CustomAttributes, methodDefinition.CustomAttributes);
            FillGenericParameters(codeMemberMethod.TypeParameters, methodDefinition.GenericParameters);
            codeMemberMethod.ReturnType = new CodeTypeReference(GetTypeReferenceName(methodDefinition.ReturnType));
            List<ParameterDefinition> outParameters = new List<ParameterDefinition>();
            foreach(var parameter in methodDefinition.Parameters) {
                CodeParameterDeclarationExpression p = GetParameterExpression(parameter);
                if(parameter.IsOut) {
                    outParameters.Add(parameter);
                }
                codeMemberMethod.Parameters.Add(p);
            }
            if(methodDefinition.HasBody) {
                foreach(var outParameter in outParameters) {
                    CodeAssignStatement assign = new CodeAssignStatement(
                        new CodeArgumentReferenceExpression(outParameter.Name),
                        GetDefalutValue(outParameter.ParameterType));
                    codeMemberMethod.Statements.Add(assign);
                }
                if(methodDefinition.ReturnType.MetadataType != MetadataType.Void) {
                    codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(GetDefalutValue(methodDefinition.ReturnType)));
                }
            }
            return codeMemberMethod;
        }

        private static MemberAttributes GetMemberAttributes(MethodDefinition methodDefinition) {
            MemberAttributes memberAttributes = 0;
            if(methodDefinition.IsAbstract) {
                memberAttributes |= MemberAttributes.Abstract;
            }
            if(!methodDefinition.IsVirtual) {
                memberAttributes |= MemberAttributes.Final;
            }
            else if(!methodDefinition.IsNewSlot) {
                memberAttributes |= MemberAttributes.Override;
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

        private void FillGenericParameters(CodeTypeParameterCollection codes, IEnumerable<GenericParameter> defines) {
            foreach(var genericParameter in defines) {
                CodeTypeParameter t = new CodeTypeParameter(genericParameter.FullName);
                if(genericParameter.IsContravariant) {
                    t.HasConstructorConstraint = true;
                }
                foreach(var constraint in genericParameter.Constraints) {
                    t.Constraints.Add(new CodeTypeReference(constraint.FullName));
                }
                FillCustomAttribute(t.CustomAttributes, genericParameter.CustomAttributes);
                codes.Add(t);
            }
        }

        private static HashSet<string> filterAttributes = new HashSet<string>() {
            "System.Diagnostics.ConditionalAttribute",
            "System.Security.SecuritySafeCriticalAttribute",
            "System.Security.SecurityCriticalAttribute",
        };

        private bool IsAttributeEnable(CustomAttribute attribute) {
            if(!IsEnableType(attribute.AttributeType.Resolve())) {
                return false;
            }

            if(filterAttributes.Contains(attribute.AttributeType.FullName)) {
                return false;
            }

            return true;
        }

        private void FillCustomAttribute(CodeAttributeDeclarationCollection codes, IEnumerable<CustomAttribute> defines) {
            foreach(var customAttribute in defines.Where(IsAttributeEnable)) {
                List<CodeAttributeArgument> codeAttributeArguments = new List<CodeAttributeArgument>();
                foreach(var attributeArgument in customAttribute.ConstructorArguments) {
                    CodeExpression codeExpress;
                    if(attributeArgument.Type.MetadataType == MetadataType.String) {
                        codeExpress = new CodePrimitiveExpression(attributeArgument.Value);
                    }
                    else if(attributeArgument.Type.IsValueType) {
                        if(attributeArgument.Value.GetType().FullName == attributeArgument.Type.FullName) {
                            codeExpress = new CodePrimitiveExpression(attributeArgument.Value);
                        }
                        else {
                            codeExpress = new CodeCastExpression(attributeArgument.Type.FullName, new CodePrimitiveExpression(attributeArgument.Value));
                        }
                    }
                    else if(attributeArgument.Type.FullName == "System.Type") {
                        codeExpress = new CodeTypeOfExpression(attributeArgument.Type.FullName);
                    }
                    else if(attributeArgument.Value is CustomAttributeArgument) {
                        CustomAttributeArgument customAttributeArgument = (CustomAttributeArgument)attributeArgument.Value;
                        if(customAttributeArgument.Value.GetType().FullName == customAttributeArgument.Type.FullName) {
                            codeExpress = new CodePrimitiveExpression(customAttributeArgument.Value);
                        }
                        else {
                            codeExpress = new CodeCastExpression(customAttributeArgument.Type.FullName, new CodePrimitiveExpression(customAttributeArgument.Value));
                        }
                    }
                    else {
                        throw new Exception();
                    }
                    codeAttributeArguments.Add(new CodeAttributeArgument(codeExpress));
                }
                foreach(var attributeNamedArgument in customAttribute.Fields.Concat(customAttribute.Properties)) {
                    codeAttributeArguments.Add(new CodeAttributeArgument(attributeNamedArgument.Name, new CodePrimitiveExpression(attributeNamedArgument.Argument.Value)));
                }
                CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(customAttribute.AttributeType.FullName, codeAttributeArguments.ToArray());
                codes.Add(codeAttributeDeclaration);
            }
        }

        private static MemberAttributes GetFieldAttribute(FieldDefinition fieldDefinition) {
            MemberAttributes memberAttributes = 0;
            if(fieldDefinition.HasConstant) {
                memberAttributes |= MemberAttributes.Const;
            }
            else if(fieldDefinition.IsStatic) {
                memberAttributes |= MemberAttributes.Static;
            }
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

        private static string GetFieldName(FieldDefinition fieldDefinition) {
            string name = GetTypeReferenceName(fieldDefinition.FieldType);
            if(fieldDefinition.IsVolatile()) {
                name = name.Replace("modreq(System.Runtime.CompilerServices.IsVolatile)", "");
            }
            return name;
        }

        private CodeMemberField GetCodeMemberField(FieldDefinition fieldDefinition) {
            CodeMemberField codeMemberField = new CodeMemberField(GetFieldName(fieldDefinition), fieldDefinition.Name);
            codeMemberField.Attributes = GetFieldAttribute(fieldDefinition);
            if(fieldDefinition.HasConstant) {
                codeMemberField.InitExpression = new CodePrimitiveExpression(fieldDefinition.Constant);
            }
            FillCustomAttribute(codeMemberField.CustomAttributes, fieldDefinition.CustomAttributes);
            return codeMemberField;
        }

        private CodeMemberProperty GetCodeMemberProperty(PropertyDefinition propertyDefinition) {
            CodeMemberProperty codeMemberProperty = new CodeMemberProperty() {
                Type = new CodeTypeReference(GetTypeReferenceName(propertyDefinition.PropertyType)),
                Name = propertyDefinition.Name,
                Attributes = GetMemberAttributes(propertyDefinition.GetMethod),
            };
            FillCustomAttribute(codeMemberProperty.CustomAttributes, propertyDefinition.CustomAttributes);
            if(propertyDefinition.IsAutoProperty()) {
                CodeAttributeDeclaration protoMemberAttribute = new CodeAttributeDeclaration("Bridge.FieldProperty");
                codeMemberProperty.CustomAttributes.Add(protoMemberAttribute);
            }
            codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(GetDefalutValue(propertyDefinition.PropertyType)));
            if(propertyDefinition.SetMethod != null && !propertyDefinition.SetMethod.IsPrivate) {
                CodeAssignStatement assign = new CodeAssignStatement(
                    new CodeArgumentReferenceExpression("value"),
                    GetDefalutValue(propertyDefinition.PropertyType));
                codeMemberProperty.SetStatements.Add(assign);
            }
            return codeMemberProperty;
        }
    }
}
