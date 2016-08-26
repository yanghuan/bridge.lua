using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using System.Linq;
using System.Collections.Generic;

namespace Bridge.Translator.Lua
{
    public class Block : AbstractEmitterBlock
    {
        public Block(IEmitter emitter, BlockStatement blockStatement)
            : base(emitter, blockStatement)
        {
            this.Emitter = emitter;
            this.BlockStatement = blockStatement;
        }

        private BlockStatement BlockStatement
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            this.EmitBlock();
        }

        public void EmitBlock()
        {
            this.BeginEmitBlock();
            bool isBlockStatement = this.BlockStatement.Parent is BlockStatement;
            if(isBlockStatement) {
                this.BeginDoBlock();
            }
            this.DoEmitBlock();
            if(isBlockStatement) {
                this.EndCodeBlock();
                this.WriteNewLine();
            }
            this.EndEmitBlock();
        }

        public void DoEmitBlock()
        {
            foreach(var statement in this.BlockStatement.Children) {
                statement.AcceptVisitor(this.Emitter);
            }
        }

        private Dictionary<string, string> prevNamesMap_;

        public void EndEmitBlock()
        {
            this.PopLocals();
            this.ClearLocalsNamesMap(prevNamesMap_);
        }

        public void BeginEmitBlock()
        {
            this.PushLocals();
            prevNamesMap_ = this.BuildLocalsNamesMap();
        }
    }
}
