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
            var resolveResult = this.Emitter.Resolver.ResolveNode(conditionalExpression.TrueExpression, this.Emitter);
            bool isNotNilOrFail = resolveResult.Type.IsReferenceType == false && resolveResult.Type.FullName != "System.Boolean";
            if(isNotNilOrFail) {
                conditionalExpression.Condition.AcceptVisitor(this.Emitter);
                this.Write(" and ");
                conditionalExpression.TrueExpression.AcceptVisitor(this.Emitter);
                this.Write(" or ");
                conditionalExpression.FalseExpression.AcceptVisitor(this.Emitter);
            }
            else {
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
            }
        }
    }
}
