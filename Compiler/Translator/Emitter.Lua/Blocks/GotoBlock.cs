using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.NRefactory.CSharp;
using Bridge.Contract;

namespace Bridge.Translator.Lua {
    public sealed class GotoBlock : AbstractEmitterBlock {
        private GotoStatement gotoStatement_;

        public GotoBlock(IEmitter emitter, GotoStatement gotoStatement) : base(emitter, gotoStatement) {
            gotoStatement_ = gotoStatement;
        }

        protected override void DoEmit() {
            this.Write("goto ", gotoStatement_.Label);
            this.WriteNewLine();
        }
    }

    public sealed class LabelBlock : AbstractEmitterBlock {
        private LabelStatement labelStatement_;

        public LabelBlock(IEmitter emitter, LabelStatement labelStatement) : base(emitter, labelStatement) {
            labelStatement_ = labelStatement;
        }

        protected override void DoEmit() {
            this.Write("::", labelStatement_.Label, "::");
            this.WriteNewLine();
        }
    }
}
