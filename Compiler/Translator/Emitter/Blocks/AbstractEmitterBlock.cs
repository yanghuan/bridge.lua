using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator
{
    public abstract partial class AbstractEmitterBlock : IAbstractEmitterBlock
    {
        private AstNode previousNode;

        public AbstractEmitterBlock(IEmitter emitter, AstNode node)
        {
            this.Emitter = emitter;
            this.previousNode = this.Emitter.Translator.EmitNode;
            this.Emitter.Translator.EmitNode = node;
        }

        protected abstract void DoEmit();

        public AstNode PreviousNode
        {
            get
            {
                return this.previousNode;
            }
        }

        public IEmitter Emitter
        {
            get;
            set;
        }

        public virtual void Emit()
        {
            this.BeginEmit();
            this.DoEmit();
            this.EndEmit();
        }

        protected virtual void BeginEmit()
        {
        }

        protected virtual void EndEmit()
        {
            this.Emitter.Translator.EmitNode = this.previousNode;
        }

        public virtual void EmitBlockOrIndentedLine(AstNode node)
        {
            bool block = node is BlockStatement;

            if (!block)
            {
                this.WriteNewLine();
                this.Indent();
            }
            else
            {
                this.WriteSpace();
            }

            node.AcceptVisitor(this.Emitter);

            if (!block)
            {
                this.Outdent();
            }
        }

        public bool NoValueableSiblings(AstNode node)
        {
            while (node.NextSibling != null)
            {
                var sibling = node.NextSibling;

                if (sibling is NewLineNode || sibling is CSharpTokenNode || sibling is Comment)
                {
                    node = sibling;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        protected Expression[] GetAwaiters(AstNode node)
        {
            var awaitSearch = new AwaitSearchVisitor();
            node.AcceptVisitor(awaitSearch);

            return awaitSearch.GetAwaitExpressions().ToArray();
        }

        protected bool IsDirectAsyncBlockChild(AstNode node)
        {
            var block = node.GetParent<BlockStatement>();

            if (block != null && (block.Parent is MethodDeclaration || block.Parent is AnonymousMethodExpression || block.Parent is LambdaExpression))
            {
                return true;
            }

            return false;
        }

        protected IAsyncStep WriteAwaiter(AstNode node)
        {
            var index = System.Array.IndexOf(this.Emitter.AsyncBlock.AwaitExpressions, node) + 1;
            this.Write("$task" + index + " = ");

            var oldValue = this.Emitter.ReplaceAwaiterByVar;
            this.Emitter.ReplaceAwaiterByVar = true;
            node.AcceptVisitor(this.Emitter);
            this.Emitter.ReplaceAwaiterByVar = oldValue;

            this.WriteSemiColon();
            this.WriteNewLine();
            this.Write("$step = " + this.Emitter.AsyncBlock.Step + ";");
            this.WriteNewLine();

            if (this.Emitter.AsyncBlock.IsTaskReturn)
            {
                this.Write("$task" + index + ".continueWith($asyncBody);");
            }
            else
            {
                this.Write("$task" + index + ".continueWith($asyncBody, true);");
            }

            this.WriteNewLine();
            this.Write("return;");

            var asyncStep = this.Emitter.AsyncBlock.AddAsyncStep(index);

            if (this.Emitter.AsyncBlock.EmittedAsyncSteps != null)
            {
                this.Emitter.AsyncBlock.EmittedAsyncSteps.Add(asyncStep);
            }

            return asyncStep;
        }

        protected void WriteAwaiters(AstNode node)
        {
            var awaiters = this.Emitter.IsAsync && !node.IsNull ? this.GetAwaiters(node) : null;

            if (awaiters != null && awaiters.Length > 0)
            {
                var oldValue = this.Emitter.AsyncExpressionHandling;
                this.Emitter.AsyncExpressionHandling = true;

                foreach (var awaiter in awaiters)
                {
                    this.WriteAwaiter(awaiter);
                }

                this.Emitter.AsyncExpressionHandling = oldValue;
            }
        }

        public AstNode GetParentFinallyBlock(AstNode node, bool stopOnLoops)
        {
            var insideTryFinally = false;
            var target = node.GetParent(n =>
            {
                if (n is LambdaExpression || n is AnonymousMethodExpression || n is MethodDeclaration)
                {
                    return true;
                }

                if (stopOnLoops && (n is WhileStatement || n is ForeachStatement || n is ForStatement || n is DoWhileStatement))
                {
                    return true;
                }

                if (n is TryCatchStatement && !((TryCatchStatement)n).FinallyBlock.IsNull)
                {
                    insideTryFinally = true;
                    return true;
                }

                return false;
            });

            return insideTryFinally ? ((TryCatchStatement)target).FinallyBlock : null;
        }
    }
}
