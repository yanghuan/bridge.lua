using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator.Lua
{
    public class ConditionalBlock : AbstractEmitterBlock
    {
        public ConditionalBlock(IEmitter emitter, ConditionalExpression conditionalExpression)
            : base(emitter, conditionalExpression)
        {
            this.Emitter = emitter;
            this.ConditionalExpression = conditionalExpression;
        }

        public ConditionalExpression ConditionalExpression
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            var conditionalExpression = this.ConditionalExpression;
            this.Write(LuaHelper.Root, ".ternary");
            this.WriteOpenParentheses();
            conditionalExpression.Condition.AcceptVisitor(this.Emitter);
            this.WriteComma();
            conditionalExpression.TrueExpression.AcceptVisitor(this.Emitter);
            this.WriteComma();
            conditionalExpression.FalseExpression.AcceptVisitor(this.Emitter);
            this.WriteCloseParentheses();
        }
    }
}
