using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator.Lua
{
    public class VisitorCustomEventBlock : AbstractMethodBlock
    {
        public VisitorCustomEventBlock(IEmitter emitter, CustomEventDeclaration customEventDeclaration)
            : base(emitter, customEventDeclaration)
        {
            this.Emitter = emitter;
            this.CustomEventDeclaration = customEventDeclaration;
        }

        public CustomEventDeclaration CustomEventDeclaration
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            this.EmitPropertyMethod(this.CustomEventDeclaration, this.CustomEventDeclaration.AddAccessor, false);
            this.EmitPropertyMethod(this.CustomEventDeclaration, this.CustomEventDeclaration.RemoveAccessor, true);
        }

        protected virtual void EmitPropertyMethod(CustomEventDeclaration customEventDeclaration, Accessor accessor, bool remover)
        {
            if (!accessor.IsNull)
            {
                this.EnsureComma();

                this.ResetLocals();

                var prevMap = this.BuildLocalsMap();
                var prevNamesMap = this.BuildLocalsNamesMap();

                this.AddLocals(new ParameterDeclaration[] { new ParameterDeclaration { Name = "value" } }, accessor.Body);
                XmlToJsDoc.EmitComment(this, this.CustomEventDeclaration);
                var overloads = OverloadsCollection.Create(this.Emitter, customEventDeclaration, remover);

                this.Write((remover ? "remove" : "add") + overloads.GetOverloadName());
                this.WriteEqualsSign();
                this.WriteFunction();
                this.WriteOpenParentheses();
                this.Write("value");
                this.WriteCloseParentheses();
                this.WriteSpace();

                accessor.Body.AcceptVisitor(this.Emitter);

                this.ClearLocalsMap(prevMap);
                this.ClearLocalsNamesMap(prevNamesMap);
                this.Emitter.Comma = true;
            }
        }
    }
}