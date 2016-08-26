using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using System.Linq;

namespace Bridge.Translator.Lua
{
    public class VisitorPropertyBlock : AbstractMethodBlock
    {
        public VisitorPropertyBlock(IEmitter emitter, PropertyDeclaration propertyDeclaration)
            : base(emitter, propertyDeclaration)
        {
            this.Emitter = emitter;
            this.PropertyDeclaration = propertyDeclaration;
        }

        public PropertyDeclaration PropertyDeclaration
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            if (this.PropertyDeclaration.Getter.Body.IsNull && this.PropertyDeclaration.Setter.Body.IsNull)
            {
                return;
            }

            this.EmitPropertyMethod(this.PropertyDeclaration, this.PropertyDeclaration.Getter, false);
            this.EmitPropertyMethod(this.PropertyDeclaration, this.PropertyDeclaration.Setter, true);
        }

        protected virtual void EmitPropertyMethod(PropertyDeclaration propertyDeclaration, Accessor accessor, bool setter)
        {
            var memberResult = this.Emitter.Resolver.ResolveNode(propertyDeclaration, this.Emitter) as MemberResolveResult;

            if (memberResult != null &&
                (memberResult.Member.Attributes.Any(a => a.AttributeType.FullName == "Bridge.FieldPropertyAttribute" ||
                    a.AttributeType.FullName == "Bridge.IgnoreAttribute" ||
                    a.AttributeType.FullName == "Bridge.ExternalAttribute") ||
                (propertyDeclaration.Getter.IsNull && propertyDeclaration.Setter.IsNull)))
            {
                return;
            }

            if (!accessor.IsNull && this.Emitter.GetInline(accessor) == null)
            {
                //this.EnsureComma();
                this.EnsureNewLine();
                this.ResetLocals();

                var prevMap = this.BuildLocalsMap();
                var prevNamesMap = this.BuildLocalsNamesMap();

                if (setter)
                {
                    this.AddLocals(new ParameterDeclaration[] { new ParameterDeclaration { Name = "value" } }, accessor.Body);
                }

                XmlToJsDoc.EmitComment(this, this.PropertyDeclaration);
                var overloads = OverloadsCollection.Create(this.Emitter, propertyDeclaration, setter);
                string name = overloads.GetOverloadName();
                name = setter ? "set" + name : "get" + name;
                TransformCtx.CurClassMethodNames.Add(new TransformCtx.MethodInfo() {
                    Name = name,
                    IsPrivate = propertyDeclaration.HasModifier(Modifiers.Private),
                });

                this.Write(name);
                this.WriteEqualsSign();
                this.WriteFunction();
                this.WriteOpenParentheses();
                if(propertyDeclaration.HasModifier(Modifiers.Static)) {
                    this.Write(setter ? "value" : "");
                }
                else {
                    this.WriteThis();
                    this.Write(setter ? ", value" : "");
                }
                this.WriteCloseParentheses();
                this.WriteSpace();
                this.BeginFunctionBlock();

                var script = this.Emitter.GetScript(accessor);
                if (script == null)
                {
                    MarkTempVars();
                    accessor.Body.AcceptVisitor(this.Emitter);
                    EmitTempVars();
                }
                else
                {
                    foreach (var line in script)
                    {
                        this.Write(line);
                        this.WriteNewLine();
                    }
                }

                this.EndFunctionBlock();
                this.ClearLocalsMap(prevMap);
                this.ClearLocalsNamesMap(prevNamesMap);
                this.Emitter.Comma = true;
            }
        }
    }
}