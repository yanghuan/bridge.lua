
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator.Lua {
    public sealed class TypeofBlock : AbstractEmitterBlock {
        public TypeOfExpression TypeOfExpression { get; private set; }

        public TypeofBlock(IEmitter emitter, TypeOfExpression typeOfExpression)
            : base(emitter, typeOfExpression)
        {
            this.Emitter = emitter;
            this.TypeOfExpression = typeOfExpression;
        }

        protected override void DoEmit() {
            this.Write("System.typeof");
            this.WriteOpenParentheses();
            TypeOfExpression.Type.AcceptVisitor(this.Emitter);
            this.WriteCloseParentheses();
        }
    }
}
