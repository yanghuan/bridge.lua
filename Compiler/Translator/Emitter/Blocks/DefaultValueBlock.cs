using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator
{
    public class DefaultValueBlock : AbstractEmitterBlock
    {
        public DefaultValueBlock(IEmitter emitter, DefaultValueExpression defaultValueExpression)
            : base(emitter, defaultValueExpression)
        {
            this.Emitter = emitter;
            this.DefaultValueExpression = defaultValueExpression;
        }

        public DefaultValueExpression DefaultValueExpression
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            var resolveResult = this.Emitter.Resolver.ResolveNode(this.DefaultValueExpression.Type, this.Emitter);

            if (!resolveResult.IsError && resolveResult.Type.IsReferenceType.HasValue && resolveResult.Type.IsReferenceType.Value)
            {
                this.Write("null");
            }
            else
            {
                this.Write("Bridge.getDefaultValue(" + BridgeTypes.ToJsName(DefaultValueExpression.Type, this.Emitter) + ")");
            }
        }
    }
}
