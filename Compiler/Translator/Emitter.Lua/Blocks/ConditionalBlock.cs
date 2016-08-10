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

        /// <summary>
        /// http://lua-users.org/wiki/TernaryOperator
        /// </summary>
        protected override void DoEmit()
        {
            var conditionalExpression = this.ConditionalExpression;

            var tempName = this.GetTempVarName();
            this.PushWriter("{0}");

            this.Write("local ", tempName);
            this.WriteNewLine();
            this.WriteIf();
            conditionalExpression.Condition.AcceptVisitor(this.Emitter);
            this.WriteSpace();
            this.BeginIfBlock();
            this.Write(tempName, " = ");
            conditionalExpression.TrueExpression.AcceptVisitor(this.Emitter);
            this.Outdent();
            this.WriteNewLine();
            this.WriteElse();
            this.BeginCodeBlock();
            this.Write(tempName, " = ");
            conditionalExpression.FalseExpression.AcceptVisitor(this.Emitter);
            this.WriteNewLine();
            this.EndCodeBlock();

            string script = this.PopWriter(true);
            this.WriteToPrevLine(script);
            this.Write(tempName);
            this.RemoveTempVar(tempName);

            /*
            var conditionalExpression = this.ConditionalExpression;

            var tempName = this.GetTempVarName();
            this.PushWriter("{0}");

            this.Write("local ", tempName);
            this.WriteNewLine();
            this.Write("if ");
            conditionalExpression.Condition.AcceptVisitor(this.Emitter);
            this.Write(" then ");
            this.Write(tempName, " = ");
            conditionalExpression.TrueExpression.AcceptVisitor(this.Emitter);
            this.Write(" else ");
            this.Write(tempName, " = ");
            conditionalExpression.FalseExpression.AcceptVisitor(this.Emitter);
            this.Write(" end");

            string script = this.PopWriter(true);
            this.WriteToPrevLine(script);
            this.Write(tempName);
            this.RemoveTempVar(tempName);
             */

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
