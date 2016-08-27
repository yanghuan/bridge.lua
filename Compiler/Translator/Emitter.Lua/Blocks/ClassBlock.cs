using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

using Object.Net.Utilities;
using Bridge.Contract;

namespace Bridge.Translator.Lua
{
    public class ClassBlock : AbstractEmitterBlock
    {
        private System.Text.StringBuilder methods_ = new System.Text.StringBuilder();
        private int methodsPosIndex_;

        public ClassBlock(IEmitter emitter, ITypeInfo typeInfo)
            : base(emitter, typeInfo.TypeDeclaration)
        {
            this.TypeInfo = typeInfo;
        }

        public ITypeInfo TypeInfo
        {
            get;
            set;
        }

        public bool IsGeneric {
            get;
            set;
        }

        private int fieldAreaStartPostion_;

        private void WriteMethodNames(IList<TransformCtx.MethodInfo> names) {
            const int kCellCount = 8;
            foreach(var cells in names.Cut(kCellCount)) {
                this.Write("local ");
                bool isComma = false;
                foreach(var info in cells) {
                    if(isComma) {
                        this.Write(", ");
                    }
                    else {
                        isComma = true;
                    }
                    this.Write(info.Name);
                }
                this.WriteNewLine();
            }
        }

        protected override void DoEmit()
        {
            TransformCtx.CurClass = TypeInfo.Type;

            XmlToJsDoc.EmitComment(this, this.Emitter.Translator.EmitNode);
            this.EmitClassHeader();
            if (this.TypeInfo.TypeDeclaration.ClassType != ClassType.Interface)
            {
                this.EmitStaticBlock(true);
                this.EmitInstantiableBlock(true);
            }

            var noneInitNames = TransformCtx.CurClassOtherMethodNames;
            var methods = TransformCtx.CurClassOtherMethods;

            if(noneInitNames.Count > 0 || methods.Count > 0) {
                this.PushWriter("{0}");
                this.Outdent();
                this.Emitter.IsNewLine = true;
                if(noneInitNames.Count > 0) {
                    WriteMethodNames(noneInitNames);
                }
                if(methods.Count > 0) {
                    foreach(string method in TransformCtx.CurClassOtherMethods) {
                        this.Write(method);
                        this.WriteNewLine();
                    }
                }
                this.Indent();
                string content = this.PopWriter(true);
                this.Emitter.Output.Insert(methodsPosIndex_, content);
            }

            var names = TransformCtx.CurClassMethodNames.Concat(TransformCtx.CurClassOtherMethodNames.Where(i => !i.IsCtor)).Where(i => !i.IsPrivate).Select(i => i.Name).ToArray();
            if(names.Length > 0) {
                this.EnsureComma();
                int index = 0;
                foreach(string name in names) {
                    ++index;
                    this.Write(name, " = ", name);
                    if(index != names.Length) {
                        this.WriteComma(true);
                    }
                }
            }
            this.EmitClassEnd();

            TransformCtx.CurClass = null;
            TransformCtx.CurClassMethodNames.Clear();
            TransformCtx.CurClassOtherMethodNames.Clear();
            TransformCtx.CurClassOtherMethods.Clear();
        }

        private void EmitClassHeader()
        {
            var beforeDefineMethods = this.GetBeforeDefineMethods();
            if (beforeDefineMethods.Any())
            {
                foreach (var method in beforeDefineMethods)
                {
                    this.WriteNewLine();
                    this.Write(method);
                }

                this.WriteNewLine();
            }

            var topDefineMethods = this.GetTopDefineMethods();
            foreach(var method in topDefineMethods) {
                this.Emitter.EmitterOutput.TopOutput.Append(method);
            }

            string typeName, namespaceName;
            var typeDef = this.Emitter.GetTypeDefinition();
            string name = this.Emitter.Validator.GetCustomTypeName(typeDef, this.Emitter);
            if(name.IsNotEmpty()) {
                int pos = name.LastIndexOf('.');
                if(pos != -1) {
                    typeName = name.Substring(pos + 1);
                    namespaceName = name.Substring(0, pos);
                }
                else {
                    typeName = name;
                    namespaceName = null;
                }
            }
            else {
                name = BridgeTypes.DefinitionToJsName(this.TypeInfo.Type, this.Emitter);
                namespaceName = this.TypeInfo.Namespace;
                if(!string.IsNullOrEmpty(namespaceName)) {
                    if(!name.StartsWith(namespaceName)) {
                        throw new System.Exception();
                    }
                    typeName = name.Substring(namespaceName.Length + 1);
                }
                else {
                    typeName = name;
                }
            }          
            TransformCtx.NamespaceNames.Add(this.TypeInfo, namespaceName);

            string methodName;
            if(typeDef.IsEnum) {
                methodName = "enum";
            }
            else if(typeDef.IsValueType) {
                methodName = "struct";
            }
            else if(typeDef.IsInterface) {
                methodName = "interface";
            }
            else {
                methodName = "class";
            }
       
            this.Indent();
            this.Write("namespace.", methodName);
            this.WriteOpenParentheses();
            this.Write("\"", typeName, "\"");
            this.WriteComma();
            this.WriteFunction();
            this.WriteOpenParentheses();
            this.WriteCloseParentheses();
            this.BeginFunctionBlock();
            this.IsGeneric = typeDef.GenericParameters.Count > 0;
            if(this.IsGeneric) {
                this.WriteReturn(true);
                this.WriteFunction();
                this.WriteOpenParentheses();

                foreach(var p in typeDef.GenericParameters) {
                    this.EnsureComma(false);
                    this.Write(p.Name);
                    this.Emitter.Comma = true;
                }
                this.Emitter.Comma = false;
                this.WriteCloseParentheses();
                this.BeginFunctionBlock();
            }

            if(this.TypeInfo.TypeDeclaration.ClassType != ClassType.Interface) {
                this.EmitStaticBlock(false);
                this.EmitInstantiableBlock(false);
                this.Emitter.Comma = false;

                if(TransformCtx.CurClassMethodNames.Count > 0) {
                    WriteMethodNames(TransformCtx.CurClassMethodNames);
                }

                methodsPosIndex_ = this.Emitter.Output.Length;
                if(methods_.Length > 0) {
                    this.Write("");
                    this.Write(methods_);
                    this.WriteNewLine();
                }
            }

            this.WriteReturn(true);
            this.BeginBlock();
            fieldAreaStartPostion_ = this.Emitter.Output.Length;
            string extend = this.Emitter.GetTypeHierarchy();

            if(extend.IsNotEmpty() && !this.TypeInfo.IsEnum) {
                string inherits = "inherits".Ident();
                this.Write(inherits);
                this.WriteEqualsSign();

                var bridgeType = this.Emitter.BridgeTypes.Get(this.Emitter.TypeInfo);
                if(Helpers.IsTypeArgInSubclass(bridgeType.TypeDefinition, bridgeType.TypeDefinition, this.Emitter, false)) {
                    this.WriteFunction();
                    this.WriteOpenCloseParentheses(true);
                    this.WriteReturn(true);
                    this.Write(extend);
                    this.WriteSemiColon();
                    this.WriteSpace();
                    this.WriteEnd();
                }
                else {
                    this.Write(extend);
                }
                this.Emitter.Comma = true;
            }

            if(this.TypeInfo.Module != null) {
                this.WriteScope();
            }
        }

        protected virtual void WriteScope()
        {
            this.EnsureComma();
            this.Write("$scope");
            this.WriteEqualsSign();
            this.Write("exports");
            this.Emitter.Comma = true;
        }

        protected virtual void EmitStaticBlock(bool isConstructor)
        {
            if (this.TypeInfo.HasRealStatic(this.Emitter))
            {
                if(isConstructor) {
                    new ConstructorBlock(this.Emitter, this.TypeInfo, true).Emit();
                }
                else {
                    this.PushWriter("{0}");
                    new MethodBlock(this.Emitter, this.TypeInfo, true).Emit();
                    string methods = this.PopWriter(true);
                    methods_.Append(methods);
                }
            }
        }

        protected virtual void EmitInstantiableBlock(bool isConstructor)
        {
            var ctorBlock = new ConstructorBlock(this.Emitter, this.TypeInfo, false);
            if(isConstructor) {
                ctorBlock.Emit();
            }
            else {
                this.PushWriter("{0}");
                new MethodBlock(this.Emitter, this.TypeInfo, false).Emit();
                string methods = this.PopWriter(true);
                methods_.Append(methods);
            }
        }

        private bool IsFiledAreaEmpty {
            get {
                var output = this.Emitter.Output;
                bool isEmpty = true;
                for(int i = fieldAreaStartPostion_; i < output.Length; ++i) {
                    if(!char.IsWhiteSpace(output[i])) {
                        isEmpty = false;
                    }
                }
                return isEmpty;
            }
        }

        private void EmitClassEnd()
        {
            if(!IsFiledAreaEmpty) {
                this.WriteNewLine();
            }
            this.EndBlock();

            /*
            var classStr = this.Emitter.Output.ToString().Substring(this.StartPosition);
            if (Regex.IsMatch(classStr, "^\\s*,\\s*\\{\\s*\\}\\s*$", RegexOptions.Multiline))
            {
                this.Emitter.Output.Remove(this.StartPosition, this.Emitter.Output.Length - this.StartPosition);
            }*/

            if (this.IsGeneric)
            {
                this.WriteNewLine();
                this.EndFunctionBlock();
            }

            this.WriteSemiColon();

            var afterDefineMethods = this.GetAfterDefineMethods();
            foreach (var method in afterDefineMethods)
            {
                this.WriteNewLine();
                this.Write(method);
            }

            var bottomDefineMethods = this.GetBottomDefineMethods();
            foreach(var method in bottomDefineMethods) {
                this.Emitter.EmitterOutput.BottomOutput.Append(method);
            }

            this.WriteNewLine();
            this.EndFunctionBlock();
            this.WriteCloseParentheses();
            this.Outdent();
            this.WriteNewLine();
        }

        protected virtual IEnumerable<string> GetDefineMethods(string prefix, Func<MethodDeclaration, IMethod, string> fn)
        {
            var methods = this.TypeInfo.InstanceMethods;
            var attrName = "Bridge.InitAttribute";
            int value = 0;

            switch (prefix)
            {
                case "After":
                    value = 0;
                    break;
                case "Before":
                    value = 1;
                    break;
                case "Top":
                    value = 2;
                    break;
                case "Bottom":
                    value = 3;
                    break;
            }

            foreach (var methodGroup in methods)
            {
                foreach (var method in methodGroup.Value)
                {
                    foreach (var attrSection in method.Attributes)
                    {
                        foreach (var attr in attrSection.Attributes)
                        {
                            var rr = this.Emitter.Resolver.ResolveNode(attr.Type, this.Emitter);
                            if (rr.Type.FullName == attrName)
                            {
                                throw new EmitterException(attr, "Instance method cannot be Init method");
                            }
                        }
                    }
                }
            }

            methods = this.TypeInfo.StaticMethods;
            List<string> list = new List<string>();

            foreach (var methodGroup in methods)
            {
                foreach (var method in methodGroup.Value)
                {
                    MemberResolveResult rrMember = null;
                    IMethod rrMethod = null;
                    foreach (var attrSection in method.Attributes)
                    {
                        foreach (var attr in attrSection.Attributes)
                        {
                            var rr = this.Emitter.Resolver.ResolveNode(attr.Type, this.Emitter);
                            if (rr.Type.FullName == attrName)
                            {
                                int? initPosition = null;
                                if (attr.HasArgumentList)
                                {
                                    if (attr.Arguments.Count > 0)
                                    {
                                        var argExpr = attr.Arguments.First();
                                        var argrr = this.Emitter.Resolver.ResolveNode(argExpr, this.Emitter);
                                        if (argrr.ConstantValue is int)
                                        {
                                            initPosition = (int)argrr.ConstantValue;
                                        }
                                    }
                                    else
                                    {
                                        initPosition = 0; //Default InitPosition.After
                                    }
                                }
                                else
                                {
                                    initPosition = 0; //Default InitPosition.After
                                }

                                if (initPosition == value)
                                {
                                    if (rrMember == null)
                                    {
                                        rrMember = this.Emitter.Resolver.ResolveNode(method, this.Emitter) as MemberResolveResult;
                                        rrMethod = rrMember != null ? rrMember.Member as IMethod : null;
                                    }

                                    if (rrMethod != null)
                                    {
                                        if (rrMethod.TypeParameters.Count > 0)
                                        {
                                            throw new EmitterException(method, "Init method cannot be generic");
                                        }

                                        if (rrMethod.Parameters.Count > 0)
                                        {
                                            throw new EmitterException(method, "Init method should not have parameters");
                                        }

                                        if (rrMethod.ReturnType.Kind != TypeKind.Void)
                                        {
                                            throw new EmitterException(method, "Init method should not return anything");
                                        }

                                        list.Add(fn(method, rrMethod));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        protected virtual IEnumerable<string> GetBeforeDefineMethods()
        {
            return this.GetDefineMethods("Before",
                (method, rrMethod) =>
                {
                    this.PushWriter("Bridge.init(function(){0});");
                    this.ResetLocals();
                    var prevMap = this.BuildLocalsMap();
                    var prevNamesMap = this.BuildLocalsNamesMap();

                    method.Body.AcceptVisitor(this.Emitter);

                    this.ClearLocalsMap(prevMap);
                    this.ClearLocalsNamesMap(prevNamesMap);
                    return this.PopWriter(true);
                });
        }

        protected virtual IEnumerable<string> GetTopDefineMethods()
        {
            return this.GetDefineMethods("Top",
                (method, rrMethod) =>
                {
                    this.PushWriter("{0}");
                    this.ResetLocals();
                    var prevMap = this.BuildLocalsMap();
                    var prevNamesMap = this.BuildLocalsNamesMap();
                    this.Emitter.NoBraceBlock = method.Body;
                    method.Body.AcceptVisitor(this.Emitter);

                    this.ClearLocalsMap(prevMap);
                    this.ClearLocalsNamesMap(prevNamesMap);
                    return this.PopWriter(true);
                });
        }

        protected virtual IEnumerable<string> GetBottomDefineMethods()
        {
            return this.GetDefineMethods("Bottom",
                (method, rrMethod) =>
                {
                    this.PushWriter("{0}");
                    this.ResetLocals();
                    var prevMap = this.BuildLocalsMap();
                    var prevNamesMap = this.BuildLocalsNamesMap();
                    this.Emitter.NoBraceBlock = method.Body;
                    method.Body.AcceptVisitor(this.Emitter);

                    this.ClearLocalsMap(prevMap);
                    this.ClearLocalsNamesMap(prevNamesMap);
                    return this.PopWriter(true);
                });
        }

        protected virtual IEnumerable<string> GetAfterDefineMethods()
        {
            return this.GetDefineMethods("After",
                (method, rrMethod) =>
                    "Bridge.init(" + BridgeTypes.ToJsName(rrMethod.DeclaringTypeDefinition, this.Emitter) + "." +
                    this.Emitter.GetEntityName(method) + ");");
        }
    }
}
