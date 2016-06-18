using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator
{
    public class AnonymousTypeCreateBlock : AbstractObjectCreateBlock
    {
        public AnonymousTypeCreateBlock(IEmitter emitter, AnonymousTypeCreateExpression anonymousTypeCreateExpression)
            : base(emitter, anonymousTypeCreateExpression)
        {
            this.Emitter = emitter;
            this.AnonymousTypeCreateExpression = anonymousTypeCreateExpression;
        }

        public AnonymousTypeCreateExpression AnonymousTypeCreateExpression
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            this.VisitAnonymousTypeCreateExpression();
        }

        protected void VisitAnonymousTypeCreateExpression()
        {
            AnonymousTypeCreateExpression anonymousTypeCreateExpression = this.AnonymousTypeCreateExpression;

            this.WriteOpenBrace();
            this.WriteSpace();

            if (anonymousTypeCreateExpression.Initializers.Count > 0)
            {
                this.WriteObjectInitializer(anonymousTypeCreateExpression.Initializers, true);

                this.WriteSpace();
                this.WriteCloseBrace();
            }
            else
            {
                this.WriteCloseBrace();
            }
        }
    }
}
