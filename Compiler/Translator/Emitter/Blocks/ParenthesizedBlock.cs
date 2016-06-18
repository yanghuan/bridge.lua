using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator
{
    public class ParenthesizedBlock : AbstractEmitterBlock
    {
        public ParenthesizedBlock(IEmitter emitter, ParenthesizedExpression parenthesizedExpression)
            : base(emitter, parenthesizedExpression)
        {
            this.Emitter = emitter;
            this.ParenthesizedExpression = parenthesizedExpression;
        }

        public ParenthesizedExpression ParenthesizedExpression
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            var ignoreParentheses = this.IgnoreParentheses(this.ParenthesizedExpression.Expression);

            if (!ignoreParentheses)
            {
                this.WriteOpenParentheses();
            }

            this.ParenthesizedExpression.Expression.AcceptVisitor(this.Emitter);

            if (!ignoreParentheses)
            {
                this.WriteCloseParentheses();
            }
        }

        protected bool IgnoreParentheses(Expression expression)
        {
            if (expression is CastExpression)
            {
                var simpleType = ((CastExpression)expression).Type as SimpleType;

                if (simpleType != null && simpleType.Identifier == "dynamic")
                {
                    return true;
                }
            }
            return false;
        }
    }
}