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

            var iteratorVar = this.GetTempVarName();
            var iteratorName = this.AddLocal(iteratorVar, AstType.Null);
            this.PushWriter("{0}");

            this.Write("if ");
            conditionalExpression.Condition.AcceptVisitor(this.Emitter);
            this.Write(" then ");
            this.Write(iteratorName, " = ");
            conditionalExpression.TrueExpression.AcceptVisitor(this.Emitter);
            this.Write(" else ");
            this.Write(iteratorName, " = ");
            conditionalExpression.FalseExpression.AcceptVisitor(this.Emitter);

            string script = this.PopWriter(true);
            this.WriteToPrevLine(script);
            this.Write(iteratorName);

            /*
            var conditionalExpression = this.ConditionalExpression;
            this.Write(LuaHelper.Root, ".ternary");
            this.WriteOpenParentheses();
            conditionalExpression.Condition.AcceptVisitor(this.Emitter);
            this.WriteComma();
            conditionalExpression.TrueExpression.AcceptVisitor(this.Emitter);
            this.WriteComma();
            conditionalExpression.FalseExpression.AcceptVisitor(this.Emitter);
            this.WriteCloseParentheses();*/
        }
    }
}
