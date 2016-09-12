using System.Collections.Generic;
using System.Linq;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;

using Bridge.Contract;

namespace Bridge.Translator.Lua
{
    public partial class ConstructorBlock : AbstractMethodBlock, IConstructorBlock
    {
        public ConstructorBlock(IEmitter emitter, ITypeInfo typeInfo, bool staticBlock)
            : base(emitter, typeInfo.TypeDeclaration)
        {
            this.Emitter = emitter;
            this.TypeInfo = typeInfo;
            this.StaticBlock = staticBlock;
        }

        public ITypeInfo TypeInfo
        {
            get;
            set;
        }

        public bool StaticBlock
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            if (this.StaticBlock)
            {
                this.EmitCtorForStaticClass();
            }
            else
            {
                this.EmitCtorForInstantiableClass();
            }
        }

        private bool isInitExists_;

        protected virtual IEnumerable<string> GetInjectors()
        {
            var handlers = this.GetEvents();
            var aspects = this.Emitter.Plugins.GetConstructorInjectors(this);
            return aspects.Concat(handlers);
        }

        protected virtual void EmitCtorForStaticClass()
        {
            var ctor = this.TypeInfo.StaticCtor;
            if (ctor != null && ctor.Body.HasChildren)
            {
                this.PushWriter("{0}");
                this.Outdent();
                string methodName = "staticCtor".Ident();
                this.ResetLocals();
                var prevNamesMap = this.BuildLocalsNamesMap();
                this.Write(methodName);
                this.WriteEqualsSign();
                this.WriteFunction();
                this.WriteOpenCloseParentheses(true);

                this.BeginFunctionBlock();
                var beginPosition = this.Emitter.Output.Length;

                ctor.Body.AcceptChildren(this.Emitter);

                if (!this.Emitter.IsAsync)
                {
                    this.EmitTempVars(beginPosition, true);
                }

                this.EndFunctionBlock();
                this.ClearLocalsNamesMap(prevNamesMap);
                this.Indent();
                string methodContent = this.PopWriter(true);
                TransformCtx.CurClassOtherMethodNames.Add(new TransformCtx.MethodInfo() { Name = methodName });
                TransformCtx.CurClassOtherMethods.Add(methodContent);
            }

            var fieldBlock = new FieldBlock(this.Emitter, this.TypeInfo, true, true);
            fieldBlock.Emit();
            if (fieldBlock.WasEmitted)
            {
                this.Emitter.Comma = true;
            }

            IEnumerable<string> fieldsInjectors = fieldBlock.Injectors;
            var injectors = this.GetInjectors();
            if (this.TypeInfo.StaticConfig.HasConfigMembers || injectors.Any() || fieldsInjectors.Any())
            {
                this.EnsureComma();
                if (this.TypeInfo.StaticConfig.HasConfigMembers)
                {
                    var configBlock = new FieldBlock(this.Emitter, this.TypeInfo, true, false);
                    configBlock.Emit();
                    if (configBlock.Injectors.Count > 0)
                    {
                        injectors = configBlock.Injectors.Concat(injectors);
                    }

                    if (configBlock.WasEmitted)
                    {
                        this.Emitter.Comma = true;
                    }
                }

                if (fieldsInjectors.Any())
                {
                    injectors = fieldsInjectors.Concat(injectors);
                }

                if(injectors.Count() > 0) {
                    string methodName = "staticInit".Ident();
                    this.PushWriter("{0}");
                    this.Outdent();
                    this.Write(methodName);
                    this.WriteEqualsSign();
                    this.WriteFunction();
                    this.WriteOpenParentheses();
                    this.WriteThis();
                    this.WriteCloseParentheses();
                    this.WriteSpace();
                    this.BeginFunctionBlock();
                    foreach(var fn in injectors) {
                        this.Write(fn);
                        this.WriteNewLine();
                    }
                    this.EndFunctionBlock();
                    this.Indent();
                    string methodContent = this.PopWriter(true);
                    TransformCtx.CurClassOtherMethodNames.Add(new TransformCtx.MethodInfo() { Name = methodName });
                    TransformCtx.CurClassOtherMethods.Add(methodContent);
                }
            }
        }

        protected virtual void EmitInitMembers()
        {
            var fieldBlock = new FieldBlock(this.Emitter, this.TypeInfo, false, true);
            fieldBlock.Emit();
            if(fieldBlock.WasEmitted) {
                this.Emitter.Comma = true;
            }

            var injectors = this.GetInjectors();
            IEnumerable<string> fieldsInjectors = fieldBlock.Injectors;
            if (!this.TypeInfo.InstanceConfig.HasConfigMembers && !injectors.Any() && !fieldsInjectors.Any())
            {
                return;
            }

            if (this.TypeInfo.InstanceConfig.HasConfigMembers)
            {
                var configBlock = new FieldBlock(this.Emitter, this.TypeInfo, false, false);
                configBlock.Emit();
                if (configBlock.Injectors.Count > 0)
                {
                    injectors = configBlock.Injectors.Concat(injectors);
                }
                if (configBlock.WasEmitted)
                {
                    this.Emitter.Comma = true;
                }
            }

            if (fieldsInjectors.Any())
            {
                injectors = fieldsInjectors.Concat(injectors);
            }

            if (injectors.Count() > 0)
            {
                string initName = "init".Ident();
                if(this.TypeInfo.InstanceMethods.Any(m => m.Value.Any(subm => this.Emitter.GetEntityName(subm) == initName)) ||
                    this.TypeInfo.InstanceConfig.Fields.Any(m => m.GetName(this.Emitter) == initName)) {
                    throw new System.NotSupportedException(string.Format("has member named {0} in {1}.{2}, please refactor", initName, TypeInfo.Namespace, TypeInfo.Name));
                }

                this.PushWriter("{0}");
                this.Outdent();
                this.Write(initName);
                this.WriteEqualsSign();
                this.WriteFunction();
                this.WriteOpenParentheses();
                this.WriteThis();
                this.WriteCloseParentheses();
                this.WriteSpace();
                this.BeginFunctionBlock();

                foreach(var fn in injectors) {
                    this.Write(fn);
                    this.WriteNewLine();
                }

                this.EndFunctionBlock();
                this.Indent();
                string method = this.PopWriter(true);
                TransformCtx.CurClassOtherMethodNames.Add(new TransformCtx.MethodInfo() { Name = initName, IsPrivate = true });
                TransformCtx.CurClassOtherMethods.Add(method);
                isInitExists_ = true;
            }
        }

        private string GetCtorName(ConstructorDeclaration ctor) {
            string ctorName;
            if(this.TypeInfo.Ctors.Count > 1 && ctor.Parameters.Count > 0) {
                var overloads = OverloadsCollection.Create(this.Emitter, ctor);
                ctorName = overloads.GetOverloadName();
            }
            else {
                ctorName = "constructor";
            }
            return ctorName;
        }

        public static int GetCtorIndex(string ctorName) {
            int ctorIndex;
            if(ctorName == "constructor") {
                ctorIndex = 0;
            }
            else {
                int pos = ctorName.LastIndexOf('_');
                ctorIndex = int.Parse(ctorName.Substring(pos + 1));
            }
            return ctorIndex + 1;
        }

        public static bool IsMultiCtor(Mono.Cecil.TypeDefinition type) {
            int count = type.Methods.Count(i => i.IsConstructor && !i.IsStatic);
            return count > 1;
        }

        private static string ConvertConstructorName(string constructorName) {
            int ctorIndex = GetCtorIndex(constructorName);
            return ("ctor" + (ctorIndex == 0 ? "" : ctorIndex.ToString())).Ident();
        }

        private sealed class CtorInfo {
            public ConstructorDeclaration ConstructorDeclaration { get; private set; }
            public string ConstructorName { get; private set; }
            public string CtorName { get; private set; }

            public CtorInfo(ConstructorDeclaration constructorDeclaration, string constructorName) {
                ConstructorDeclaration = constructorDeclaration;
                ConstructorName = constructorName;
                CtorName = ConvertConstructorName(ConstructorName);
            }
        }

        private void EmitCtorForInstantiableClass() {
            this.EmitInitMembers();

            var baseType = this.Emitter.GetBaseTypeDefinition();
            var typeDef = this.Emitter.GetTypeDefinition();
            if(typeDef.IsValueType) {
                if(!typeDef.IsEnum) {
                    this.TypeInfo.Ctors.Add(new ConstructorDeclaration {
                        Modifiers = Modifiers.Public,
                        Body = new BlockStatement()
                    });
                }
            }
            else {
                if(TypeInfo.Ctors.Count == 0 && isInitExists_) {
                    this.TypeInfo.Ctors.Add(new ConstructorDeclaration {
                        Modifiers = Modifiers.Public,
                        Body = new BlockStatement()
                    });
                }
            }

            if(TypeInfo.Ctors.Count > 0) {
                var ctorInfos = this.TypeInfo.Ctors.Select(i => new CtorInfo(i, GetCtorName(i))).ToList();
                ctorInfos.Sort((x, y) => x.ConstructorName.CompareTo(y.ConstructorName));

                this.PushWriter("{0}");
                this.Outdent();
                foreach(var info in ctorInfos) {
                    var ctor = info.ConstructorDeclaration;
                    var ctorName = info.ConstructorName;
                    TransformCtx.CurClassOtherMethodNames.Add(new TransformCtx.MethodInfo() { Name = info.CtorName, IsCtor = true });
                    if(info != ctorInfos.First()) {
                        this.WriteNewLine();
                    }
                    this.ResetLocals();
                    var prevMap = this.BuildLocalsMap();
                    var prevNamesMap = this.BuildLocalsNamesMap();
                    this.AddLocals(ctor.Parameters, ctor.Body);

                    XmlToJsDoc.EmitComment(this, ctor);
                    this.Write(info.CtorName);
                    this.WriteEqualsSign();
                    this.WriteFunction();

                    this.EmitMethodParameters(ctor.Parameters, ctor);

                    this.WriteSpace();
                    this.BeginFunctionBlock();

                    var requireNewLine = false;
                    if(baseType != null && !this.Emitter.Validator.IsIgnoreType(baseType) 
                        || (ctor.Initializer != null && ctor.Initializer.ConstructorInitializerType == ConstructorInitializerType.This)) {
                        if(requireNewLine) {
                            this.WriteNewLine();
                        }
                        this.EmitBaseConstructor(ctor, ctorName);
                        requireNewLine = true;
                    }
                    else {
                        if(isInitExists_) {
                            this.Write("__init__(this)");
                            requireNewLine = true;
                        }
                    }

                    if(ctor.Body.HasChildren) {
                        var beginPosition = this.Emitter.Output.Length;
                        if(requireNewLine) {
                            this.WriteNewLine();
                        }

                        this.ConvertParamsToReferences(ctor.Parameters);
                        ctor.Body.AcceptChildren(this.Emitter);

                        if(!this.Emitter.IsAsync) {
                            this.EmitTempVars(beginPosition, true);
                        }
                    }
                    else {
                        if(requireNewLine) {
                            this.WriteNewLine();
                        }
                    }

                    this.EndFunctionBlock();
                    this.ClearLocalsMap(prevMap);
                    this.ClearLocalsNamesMap(prevNamesMap);
                }

                this.Indent();
                string ctors = this.PopWriter(true);
                TransformCtx.CurClassOtherMethods.Add(ctors);

                this.EnsureComma();
                this.Write("ctor".Ident());
                this.WriteEqualsSign();

                if(this.TypeInfo.Ctors.Count > 1) {
                    this.WriteOpenBrace();
                    this.Indent();
                    this.WriteNewLine();
                }

                foreach(var info in ctorInfos) {
                    if(info != ctorInfos.First()) {
                        this.WriteComma(true);
                    }
                    this.Write(info.CtorName);
                }

                if(this.TypeInfo.Ctors.Count > 1) {
                    this.Outdent();
                    this.WriteNewLine();
                    this.WriteCloseBrace();
                }
                this.Emitter.Comma = true;
            }
        }

        protected virtual void EmitBaseConstructor(ConstructorDeclaration ctor, string ctorName)
        {
            var initializer = ctor.Initializer != null && !ctor.Initializer.IsNull ? ctor.Initializer : new ConstructorInitializer()
            {
                ConstructorInitializerType = ConstructorInitializerType.Base
            };

            bool appendScope = false;
            if (initializer.ConstructorInitializerType == ConstructorInitializerType.Base)
            {
                var baseType = this.Emitter.GetBaseTypeDefinition();
                var baseName = "constructor";
                if (ctor.Initializer != null && !ctor.Initializer.IsNull)
                {
                    var member = ((InvocationResolveResult)this.Emitter.Resolver.ResolveNode(ctor.Initializer, this.Emitter)).Member;
                    var overloads = OverloadsCollection.Create(this.Emitter, member);
                    if (overloads.HasOverloads)
                    {
                        baseName = overloads.GetOverloadName();
                    }
                }

                string baseTypName;
                if(this.TypeInfo.GetBaseTypes(this.Emitter).Any()) {
                    baseTypName = BridgeTypes.ToJsName(this.TypeInfo.GetBaseClass(this.Emitter), this.Emitter);
                }
                else {
                    baseTypName = BridgeTypes.ToJsName(baseType, this.Emitter);
                }
                this.Write(baseTypName);
                this.WriteDot();
                this.Write("__ctor__");
                if(IsMultiCtor(baseType)) {
                    int ctorIndex = GetCtorIndex(baseName);
                    this.Write('[', ctorIndex, ']');
                }
                appendScope = true;
            }
            else
            {
                var baseName = "constructor";
                var member = ((InvocationResolveResult)this.Emitter.Resolver.ResolveNode(ctor.Initializer, this.Emitter)).Member;
                var overloads = OverloadsCollection.Create(this.Emitter, member);
                if (overloads.HasOverloads)
                {
                    baseName = overloads.GetOverloadName();
                }
                this.Write(ConvertConstructorName(baseName));
                appendScope = true;
            }

            this.WriteOpenParentheses();

            if (appendScope)
            {
                this.WriteThis();
                if (initializer.Arguments.Count > 0)
                {
                    this.WriteComma();
                }
            }

            var args = new List<Expression>(initializer.Arguments);
            for (int i = 0; i < args.Count; i++)
            {
                args[i].AcceptVisitor(this.Emitter);
                if (i != (args.Count - 1))
                {
                    this.WriteComma();
                }
            }

            this.WriteCloseParentheses();
            this.WriteSemiColon();
            if(isInitExists_ && initializer.ConstructorInitializerType == ConstructorInitializerType.Base) {
                this.WriteNewLine();
                this.Write("__init__(this)");
            }
        }
    }
}
