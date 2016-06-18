using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator.Lua
{
    public class ContinueBlock : AbstractEmitterBlock
    {
        public ContinueBlock(IEmitter emitter, ContinueStatement continueStatement)
            : base(emitter, continueStatement)
        {
            this.Emitter = emitter;
            this.ContinueStatement = continueStatement;
        }

        public ContinueStatement ContinueStatement
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            if (this.Emitter.JumpStatements != null)
            {
                var finallyNode = this.GetParentFinallyBlock(this.ContinueStatement, true);

                if (finallyNode != null)
                {
                    var hashcode = finallyNode.GetHashCode();
                    this.Emitter.AsyncBlock.JumpLabels.Add(new AsyncJumpLabel
                    {
                        Node = finallyNode,
                        Output = this.Emitter.Output
                    });
                    this.Write("$step = ${" + hashcode + "};");
                    this.WriteNewLine();
                    this.Write("$jumpFromFinally = ");
                    this.Emitter.JumpStatements.Add(new JumpInfo(this.Emitter.Output, this.Emitter.Output.Length, false));
                    this.WriteSemiColon();
                    this.WriteNewLine();
                }
                else
                {
                    this.Write("$step = ");
                    this.Emitter.JumpStatements.Add(new JumpInfo(this.Emitter.Output, this.Emitter.Output.Length, false));

                    this.WriteSemiColon();
                    this.WriteNewLine();
                }
            }

            if (this.Emitter.ReplaceJump && this.Emitter.JumpStatements == null)
            {
                this.Write("return {jump:1}");
            }
            else
            {
                this.Write("continue = true; break");
            }

            this.WriteSemiColon();
            this.WriteNewLine();
        }

        private sealed class ContinueSearchVisitor : DepthFirstAstVisitor {
            public bool Found { get; set; }
            public override void VisitContinueStatement(ContinueStatement continueStatement) {
                this.Found = true;
            }
        }

        public static bool HasContinue(AbstractEmitterBlock block, AstNode node) {
            var visitor = new ContinueSearchVisitor();
            node.AcceptVisitor(visitor);
            bool has = visitor.Found;
            if(has) {
                EmitBeginContinue(block);
            }
            return has;
        }

        /// <summary>
        /// http://lua-users.org/wiki/ContinueProposal
        /// </summary>
        /// <param name="block"></param>
        private static void EmitBeginContinue(AbstractEmitterBlock block) {
            block.WriteVar(true);
            block.Write("continue");
            block.WriteNewLine();
            block.BeginBlock("repeat");
        }

        public static void EmitEndContinue(AbstractEmitterBlock block) {
            block.Write("continue = true");
            block.WriteNewLine();
            block.Outdent();
            block.Write("until 1");
            block.WriteNewLine();
            block.Write("if not continue then break end");
            block.WriteNewLine();
        }
    }
}
