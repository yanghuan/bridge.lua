using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace Bridge.Translator.Lua
{
    public class PrimitiveExpressionBlock : ConversionBlock
    {
        public PrimitiveExpressionBlock(IEmitter emitter, PrimitiveExpression primitiveExpression)
            : base(emitter, primitiveExpression)
        {
            this.Emitter = emitter;
            this.PrimitiveExpression = primitiveExpression;
        }

        public PrimitiveExpression PrimitiveExpression
        {
            get;
            set;
        }

        protected override Expression GetExpression()
        {
            return this.PrimitiveExpression;
        }

        protected override void EmitConversionExpression()
        {
            if (this.PrimitiveExpression.IsNull)
            {
                return;
            }

            this.WriteScript(this.PrimitiveExpression.Value);
        }
    }
}