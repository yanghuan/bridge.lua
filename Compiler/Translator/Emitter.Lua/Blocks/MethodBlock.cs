using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using Object.Net.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Translator.Lua
{
    public class MethodBlock : AbstractEmitterBlock
    {
        public MethodBlock(IEmitter emitter, ITypeInfo typeInfo, bool staticBlock)
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
                this.EmitMethods(this.TypeInfo.StaticMethods, this.TypeInfo.StaticProperties, this.TypeInfo.Operators);
            }
            else
            {
                this.EmitMethods(this.TypeInfo.InstanceMethods, this.TypeInfo.InstanceProperties, null);
            }
        }

        protected virtual void EmitMethods(Dictionary<string, List<MethodDeclaration>> methods, Dictionary<string, List<EntityDeclaration>> properties, Dictionary<OperatorType, List<OperatorDeclaration>> operators)
        {
            var names = new List<string>(properties.Keys);

            foreach (var name in names)
            {
                var props = properties[name];

                foreach (var prop in props)
                {
                    if (prop is PropertyDeclaration)
                    {
                        this.Emitter.VisitPropertyDeclaration((PropertyDeclaration)prop);
                    }
                    else if (prop is CustomEventDeclaration)
                    {
                        this.Emitter.VisitCustomEventDeclaration((CustomEventDeclaration)prop);
                    }
                    else if (prop is IndexerDeclaration)
                    {
                        this.Emitter.VisitIndexerDeclaration((IndexerDeclaration)prop);
                    }
                }
            }

            names = new List<string>(methods.Keys);

            foreach (var name in names)
            {
                this.EmitMethodsGroup(methods[name]);
            }

            if (operators != null)
            {
                var ops = new List<OperatorType>(operators.Keys);

                foreach (var op in ops)
                {
                    this.EmitOperatorGroup(operators[op]);
                }
            }

            if (this.TypeInfo.ClassType == ClassType.Struct)
            {
                if (!this.StaticBlock)
                {
                    this.EmitStructMethods();
                }
                else
                {
                    var typeDef = this.Emitter.GetTypeDefinition();
                    string structName = this.Emitter.Validator.GetCustomTypeName(typeDef, this.Emitter);

                    if (structName.IsEmpty())
                    {
                        structName = BridgeTypes.ToJsName(this.TypeInfo.Type, this.Emitter);
                    }

                    //this.EnsureComma();
                    this.EnsureNewLine();
                    const string functionName = TransformCtx.DefaultString;
                    this.Write(functionName, " = function () return " + structName + CtorOpenCloseParentheses + " end");
                    this.Emitter.Comma = true;
                    TransformCtx.CurClassMethodNames.Add(new TransformCtx.MethodInfo() { Name = functionName });
                }
            }
        }

        private string CtorOpenCloseParentheses {
            get {
                return this.TypeInfo.Ctors.Count > 1 ? "(1)" : "()";
            }
        }

        protected virtual void EmitStructMethods()
        {
            var typeDef = this.Emitter.GetTypeDefinition();
            string structName = this.Emitter.Validator.GetCustomTypeName(typeDef, this.Emitter);

            if (structName.IsEmpty())
            {
                structName = BridgeTypes.ToJsName(this.TypeInfo.Type, this.Emitter);
            }

            var fields = this.TypeInfo.InstanceConfig.Fields;
            var props = this.TypeInfo.InstanceConfig.Properties.Where(ent =>
            {
                var p = ent.Entity as PropertyDeclaration;

                return p != null && p.Getter != null && p.Getter.Body.IsNull && p.Setter != null && p.Setter.Body.IsNull;
            });

            var list = fields.ToList();
            list.AddRange(props);

            if (list.Count == 0)
            {
                string name = "clone".Ident();
                TransformCtx.CurClassMethodNames.Add(new TransformCtx.MethodInfo() { Name = name });

                //this.EnsureComma();
                this.EnsureNewLine();
                this.Write(name, " = function(this, to)  return this end");
                this.Emitter.Comma = true;
                return;
            }

            const string kGetHashCode = "GetHashCode"; 
            if (!this.TypeInfo.InstanceMethods.ContainsKey(kGetHashCode))
            {
                TransformCtx.CurClassMethodNames.Add(new TransformCtx.MethodInfo() { Name = kGetHashCode });

                //this.EnsureComma();
                this.EnsureNewLine();
                this.Write(kGetHashCode, " = function(this) ");
                this.BeginFunctionBlock();
                this.Write("local hash = 17");

                foreach (var field in list)
                {
                    string fieldName = field.GetName(this.Emitter);

                    this.WriteNewLine();
                    this.Write("hash = hash * 23 + ");
                    this.Write("(this." + fieldName);
                    this.Write(" == nil and 0 or ");
                    this.Write("System.Object.", kGetHashCode, "(");
                    this.Write("this." + fieldName);
                    this.Write("))");
                }

                this.WriteNewLine();
                this.Write("return hash");
                this.WriteNewLine();
                this.EndFunctionBlock();
                this.Emitter.Comma = true;
            }

            const string kEqualsObj = "EqualsObj";
            if (!this.TypeInfo.InstanceMethods.ContainsKey("Equals"))
            {
                TransformCtx.CurClassMethodNames.Add(new TransformCtx.MethodInfo() { Name = kEqualsObj });

                //this.EnsureComma();
                this.EnsureNewLine();
                this.Write(kEqualsObj, " = function (this, o) ");
                this.BeginFunctionBlock();
                this.Write("if getmetatable(o) ~= ");
                this.Write(structName);
                this.Write(" then");
                this.BeginFunctionBlock();
                this.Write("return false");
                this.WriteNewLine();
                this.EndFunctionBlock();
                this.WriteNewLine();
                this.Write("return ");

                bool and = false;
                bool isIndent = false;
                foreach (var field in list)
                {
                    string fieldName = field.GetName(this.Emitter);
                    if (and)
                    {
                        if(!isIndent) {
                            this.Indent();
                            isIndent = true;
                        }

                        this.WriteNewLine();
                        this.Write("and ");
                    }
                    else {
                        and = true;
                    }
                    this.Write("System.Object.equalsStatic(this.");
                    this.Write(fieldName);
                    this.Write(", o.");
                    this.Write(fieldName);
                    this.Write(")");
                }

                if(isIndent) {
                    this.Outdent();
                }

                this.WriteNewLine();
                this.EndFunctionBlock();
                this.Emitter.Comma = true;
            }

            TransformCtx.CurClassMethodNames.Add(new TransformCtx.MethodInfo() { Name = "clone".Ident() });

            //this.EnsureComma();
            this.EnsureNewLine();
            this.Write("clone".Ident(), " = function (this, to) ");
            this.BeginFunctionBlock();
            this.Write("local s = to or ");
            this.Write(structName);
            this.Write(CtorOpenCloseParentheses);

            foreach (var field in list)
            {
                this.WriteNewLine();
                string fieldName = field.GetName(this.Emitter);

                this.Write("s.");
                this.Write(fieldName);
                this.Write(" = this.");
                this.Write(fieldName);
            }

            this.WriteNewLine();
            this.Write("return s");
            this.WriteNewLine();
            this.EndFunctionBlock();
            this.Emitter.Comma = true;
        }

        protected void EmitEventAccessor(EventDeclaration e, VariableInitializer evtVar, bool add)
        {
            string name = evtVar.Name;

            this.Write(add ? "add" : "remove", name, " : ");
            this.WriteFunction();
            this.WriteOpenParentheses();
            this.Write("value");
            this.WriteCloseParentheses();
            this.WriteSpace();
            this.BeginFunctionBlock();
            this.WriteThis();
            this.WriteDot();
            this.Write(this.Emitter.GetEntityName(e));
            this.Write(" = ");
            this.Write(Bridge.Translator.Emitter.ROOT, ".", add ? Bridge.Translator.Emitter.DELEGATE_COMBINE : Bridge.Translator.Emitter.DELEGATE_REMOVE);
            this.WriteOpenParentheses();
            this.WriteThis();
            this.WriteDot();
            this.Write(this.Emitter.GetEntityName(e));
            this.WriteComma();
            this.WriteSpace();
            this.Write("value");
            this.WriteCloseParentheses();
            this.WriteSemiColon();
            this.WriteNewLine();
            this.EndFunctionBlock();
        }

        protected virtual void EmitMethodsGroup(List<MethodDeclaration> group)
        {
            if (group.Count == 1)
            {
                if (!group[0].Body.IsNull)
                {
                    this.Emitter.VisitMethodDeclaration(group[0]);
                }
            }
            else
            {
                var typeDef = this.Emitter.GetTypeDefinition();
                var name = group[0].Name;
                var methodsDef = typeDef.Methods.Where(m => m.Name == name);
                this.Emitter.MethodsGroup = methodsDef;
                this.Emitter.MethodsGroupBuilder = new Dictionary<int, StringBuilder>();

                foreach (var method in group)
                {
                    if (!method.Body.IsNull)
                    {
                        this.Emitter.VisitMethodDeclaration(method);
                    }
                }

                this.Emitter.MethodsGroup = null;
                this.Emitter.MethodsGroupBuilder = null;
            }
        }

        protected virtual void EmitOperatorGroup(List<OperatorDeclaration> group)
        {
            if (group.Count == 1)
            {
                if (!group[0].Body.IsNull)
                {
                    this.Emitter.VisitOperatorDeclaration(group[0]);
                }
            }
            else
            {
                var typeDef = this.Emitter.GetTypeDefinition();
                var name = group[0].Name;
                var methodsDef = typeDef.Methods.Where(m => m.Name == name);
                this.Emitter.MethodsGroup = methodsDef;
                this.Emitter.MethodsGroupBuilder = new Dictionary<int, StringBuilder>();

                foreach (var method in group)
                {
                    if (!method.Body.IsNull)
                    {
                        this.Emitter.VisitOperatorDeclaration(method);
                    }
                }

                this.Emitter.MethodsGroup = null;
                this.Emitter.MethodsGroupBuilder = null;
            }
        }
    }
}