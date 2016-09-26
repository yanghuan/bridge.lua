using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using Object.Net.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bridge.Translator.Lua
{
    public class TryCatchBlock : AbstractEmitterBlock {
        public TryCatchBlock(IEmitter emitter, TryCatchStatement tryCatchStatement)
            : base(emitter, tryCatchStatement) {
            this.Emitter = emitter;
            this.TryCatchStatement = tryCatchStatement;
        }

        public TryCatchStatement TryCatchStatement {
            get;
            set;
        }

        protected override void DoEmit() {
            var awaiters = this.Emitter.IsAsync ? this.GetAwaiters(this.TryCatchStatement) : null;

            if(awaiters != null && awaiters.Length > 0) {
                this.VisitAsyncTryCatchStatement();
            }
            else {
                this.VisitTryCatchStatement();
            }
        }

        protected void VisitAsyncTryCatchStatement() {
            TryCatchStatement tryCatchStatement = this.TryCatchStatement;

            this.Emitter.AsyncBlock.Steps.Last().JumpToStep = this.Emitter.AsyncBlock.Step;

            var tryStep = this.Emitter.AsyncBlock.AddAsyncStep();
            AsyncTryInfo tryInfo = new AsyncTryInfo();
            tryInfo.StartStep = tryStep.Step;

            this.Emitter.IgnoreBlock = tryCatchStatement.TryBlock;
            tryCatchStatement.TryBlock.AcceptVisitor(this.Emitter);
            tryStep = this.Emitter.AsyncBlock.Steps.Last();
            tryInfo.EndStep = tryStep.Step;

            List<IAsyncStep> catchSteps = new List<IAsyncStep>();

            foreach(var clause in tryCatchStatement.CatchClauses) {
                var catchStep = this.Emitter.AsyncBlock.AddAsyncStep();
                catchSteps.Add(catchStep);

                this.PushLocals();
                var varName = clause.VariableName;

                if(!String.IsNullOrEmpty(varName) && !this.Emitter.Locals.ContainsKey(varName)) {
                    varName = this.AddLocal(varName, clause.Type);
                }

                tryInfo.CatchBlocks.Add(new Tuple<string, string, int>(varName, clause.Type.IsNull ? "Bridge.Exception" : BridgeTypes.ToJsName(clause.Type, this.Emitter), catchStep.Step));

                this.Emitter.IgnoreBlock = clause.Body;
                clause.Body.AcceptVisitor(this.Emitter);
                this.PopLocals();
                this.WriteNewLine();
            }

            if(/*tryCatchStatement.CatchClauses.Count == 0 && */!this.Emitter.Locals.ContainsKey("$e")) {
                this.AddLocal("$e", AstType.Null);
            }

            IAsyncStep finalyStep = null;
            if(!tryCatchStatement.FinallyBlock.IsNull) {
                finalyStep = this.Emitter.AsyncBlock.AddAsyncStep(tryCatchStatement.FinallyBlock);
                this.Emitter.IgnoreBlock = tryCatchStatement.FinallyBlock;
                tryCatchStatement.FinallyBlock.AcceptVisitor(this.Emitter);

                var finallyNode = this.GetParentFinallyBlock(tryCatchStatement, false);

                this.WriteNewLine();

                this.WriteIf();
                this.WriteOpenParentheses();
                this.Write("$jumpFromFinally > -1");
                this.WriteCloseParentheses();
                this.WriteSpace();
                this.BeginBlock();
                if(finallyNode != null) {
                    var hashcode = finallyNode.GetHashCode();
                    this.Emitter.AsyncBlock.JumpLabels.Add(new AsyncJumpLabel {
                        Node = finallyNode,
                        Output = this.Emitter.Output
                    });
                    this.Write("$step = ${" + hashcode + "};");
                    this.WriteNewLine();
                    this.Write("continue;");
                }
                else {
                    this.Write("$step = $jumpFromFinally;");
                    this.WriteNewLine();
                    this.Write("$jumpFromFinally = null;");
                }

                this.WriteNewLine();
                this.EndBlock();

                this.WriteSpace();
                this.WriteElse();
                this.WriteIf();
                this.WriteOpenParentheses();
                this.Write("$e");
                this.WriteCloseParentheses();
                this.WriteSpace();
                this.BeginBlock();
                this.Write("$returnTask.setError($e);");
                this.WriteNewLine();
                this.WriteReturn(false);
                this.WriteSemiColon();
                this.WriteNewLine();
                this.EndBlock();

                this.WriteSpace();
                this.WriteElse();
                this.WriteIf();
                this.WriteOpenParentheses();
                this.Write("Bridge.isDefined($returnValue)");
                this.WriteCloseParentheses();
                this.WriteSpace();
                this.BeginBlock();

                if(finallyNode != null) {
                    var hashcode = finallyNode.GetHashCode();
                    this.Emitter.AsyncBlock.JumpLabels.Add(new AsyncJumpLabel {
                        Node = finallyNode,
                        Output = this.Emitter.Output
                    });
                    this.Write("$step = ${" + hashcode + "};");
                    this.WriteNewLine();
                    this.Write("continue;");
                }
                else {
                    this.Write("$returnTask.setResult($returnValue);");
                    this.WriteNewLine();
                    this.WriteReturn(false);
                    this.WriteSemiColon();
                }

                this.WriteNewLine();
                this.EndBlock();

                if(!this.Emitter.Locals.ContainsKey("$e")) {
                    this.AddLocal("$e", AstType.Null);
                }
            }

            var nextStep = this.Emitter.AsyncBlock.AddAsyncStep();
            if(finalyStep != null) {
                tryInfo.FinallyStep = finalyStep.Step;
            }

            if(finalyStep != null) {
                finalyStep.JumpToStep = nextStep.Step;
            }

            tryStep.JumpToStep = finalyStep != null ? finalyStep.Step : nextStep.Step;

            foreach(var step in catchSteps) {
                step.JumpToStep = finalyStep != null ? finalyStep.Step : nextStep.Step;
            }

            this.Emitter.AsyncBlock.TryInfos.Add(tryInfo);
        }

        protected void VisitTryCatchStatement() {
            var visitor = new ReturnSearchVisitor();
            this.TryCatchStatement.AcceptVisitor(visitor);
            bool hasRet = visitor.Found;
            if(hasRet) {
                this.WriteReturn(true);
            }
            this.Write("System.try");
            this.WriteOpenParentheses();
            this.EmitTryBlock();

            var count = this.TryCatchStatement.CatchClauses.Count;
            if(count > 0) {
                this.EmitCatchBlock();
            }
            else {
                this.Write("nil, ");
            }

            this.EmitFinallyBlock();
        }

        protected virtual void EmitTryBlock() {
            TryCatchStatement tryCatchStatement = this.TryCatchStatement;
            this.WriteFunction();
            this.WriteOpenCloseParentheses();
            this.BeginFunctionBlock();
            tryCatchStatement.TryBlock.AcceptVisitor(this.Emitter);
            this.EndFunctionBlock();
            this.WriteComma();
        }

        protected virtual void EmitFinallyBlock() {
            TryCatchStatement tryCatchStatement = this.TryCatchStatement;
            if(!tryCatchStatement.FinallyBlock.IsNull) {
                this.WriteFunction();
                this.WriteOpenCloseParentheses();
                this.BeginFunctionBlock();
                tryCatchStatement.FinallyBlock.AcceptVisitor(this.Emitter);
                this.EndFunctionBlock();
                this.WriteCloseParentheses();
                this.WriteSemiColon();
                this.WriteNewLine();
            }
        }

        protected virtual void EmitCatchBlock() {
            TryCatchStatement tryCatchStatement = this.TryCatchStatement;
            string eName = "e".Ident();

            this.WriteFunction();
            this.WriteOpenParentheses();
            this.Write(eName);
            this.WriteCloseParentheses();
            this.WriteSpace();
            this.BeginFunctionBlock();

            int index = 0;
            bool isWriteElse = true;
            bool isIfExists = false;

            foreach(CatchClause clause in tryCatchStatement.CatchClauses) {
                if(!clause.Type.IsNull) {
                    if(index != 0) {
                        this.WriteElse();
                    }

                    string exceptionType = BridgeTypes.ToJsName(clause.Type, this.Emitter);
                    if(exceptionType != "System.Exception") {
                        isIfExists = true;
                        this.WriteIf();
                        this.Write(string.Format("System.is({0}, {1})", eName, exceptionType));
                        this.BeginIfBlock();

                        if(clause.VariableName.IsNotEmpty()) {
                            this.Write(string.Format("local {0} = {1}", clause.VariableName, eName));
                            this.WriteNewLine();
                        }

                        this.Emitter.NoBraceBlock = clause.Body;
                        clause.Body.AcceptVisitor(this.Emitter);
                        this.Emitter.NoBraceBlock = null;
                        this.Outdent();
                    }
                    else {
                        isWriteElse = false;
                        if(index != 0) {
                            this.BeginFunctionBlock();
                        }

                        if(clause.VariableName.IsNotEmpty()) {
                            this.Write(string.Format("local {0} = {1}", clause.VariableName, eName));
                            this.WriteNewLine();
                        }

                        this.Emitter.NoBraceBlock = clause.Body;
                        clause.Body.AcceptVisitor(this.Emitter);
                        this.Emitter.NoBraceBlock = null;
                        this.Outdent();
                    }
                }
                else {
                    isWriteElse = false;
                    if(index != 0) {
                        this.BeginElseBlock();
                    }

                    this.Emitter.NoBraceBlock = clause.Body;
                    clause.Body.AcceptVisitor(this.Emitter);
                    this.Emitter.NoBraceBlock = null;
                    this.WriteReturn(true);
                    this.Write(LuaHelper.ReThrow);
                    this.WriteNewLine();
                    if(index != 0) {
                        this.Outdent();
                    }
                }

                ++index;
            }

            if(isWriteElse) {
                this.BeginElseBlock();
                this.WriteReturn(true);
                this.Write(LuaHelper.ReThrow);
                this.WriteNewLine();
                this.Outdent();
            }

            if(isIfExists) {
                this.Indent();
                this.EndCodeBlock();
                this.WriteNewLine();
            }

            this.EndFunctionBlock();
            if(!tryCatchStatement.FinallyBlock.IsNull) {
                this.WriteComma();
            }
            else {
                this.Write(")");
            }
            this.WriteNewLine();
        }
    }

    public class AsyncTryInfo : IAsyncTryInfo
    {
        public int StartStep
        {
            get;
            set;
        }

        public int EndStep
        {
            get;
            set;
        }

        public int FinallyStep
        {
            get;
            set;
        }

        private List<Tuple<string, string, int>> catchBlocks;

        public List<Tuple<string, string, int>> CatchBlocks
        {
            get
            {
                if (this.catchBlocks == null)
                {
                    this.catchBlocks = new List<Tuple<string, string, int>>();
                }
                return this.catchBlocks;
            }
        }
    }
}
