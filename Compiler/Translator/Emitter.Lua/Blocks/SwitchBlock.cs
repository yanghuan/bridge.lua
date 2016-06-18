using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Translator.Lua
{
    public class SwitchBlock : AbstractEmitterBlock
    {
        private string varName_;
        private bool isFirst_;
        private bool isEnd_;

        public SwitchBlock(IEmitter emitter, SwitchStatement switchStatement)
            : base(emitter, switchStatement)
        {
            this.Emitter = emitter;
            this.SwitchStatement = switchStatement;
        }

        public SwitchBlock(IEmitter emitter, SwitchSection switchSection, string varName, bool isFirst, bool isEnd)
            : base(emitter, switchSection)
        {
            this.Emitter = emitter;
            this.SwitchSection = switchSection;
            varName_ = varName;
            isFirst_ = isFirst;
            isEnd_ = isEnd;
        }

        public SwitchBlock(IEmitter emitter, CaseLabel caseLabel, string varName, bool isFirst)
            : base(emitter, caseLabel)
        {
            this.Emitter = emitter;
            this.CaseLabel = caseLabel;
            varName_ = varName;
            isFirst_ = isFirst;
        }

        public SwitchStatement SwitchStatement
        {
            get;
            set;
        }

        public SwitchSection SwitchSection
        {
            get;
            set;
        }

        public CaseLabel CaseLabel
        {
            get;
            set;
        }

        public SwitchStatement ParentAsyncSwitch
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            if (this.SwitchStatement != null)
            {
                var awaiters = this.Emitter.IsAsync ? this.GetAwaiters(this.SwitchStatement) : null;

                if (awaiters != null && awaiters.Length > 0)
                {
                    this.VisitAsyncSwitchStatement();
                }
                else
                {
                    this.VisitSwitchStatement();
                }
            }
            else if (this.SwitchSection != null)
            {
                if (this.Emitter.AsyncSwitch != null)
                {
                    throw new EmitterException(this.SwitchSection, "Async switch section must be handled by VisitAsyncSwitchStatement method");
                }
                else
                {
                    this.VisitSwitchSection();
                }
            }
            else
            {
                if (this.Emitter.AsyncSwitch != null)
                {
                    throw new EmitterException(this.CaseLabel, "Async case label must be handled by VisitAsyncSwitchStatement method");
                }
                else
                {
                    this.VisitCaseLabel();
                }
            }
        }

        protected void VisitAsyncSwitchStatement()
        {
            SwitchStatement switchStatement = this.SwitchStatement;
            this.ParentAsyncSwitch = this.Emitter.AsyncSwitch;
            this.Emitter.AsyncSwitch = switchStatement;

            this.WriteAwaiters(switchStatement.Expression);

            var oldValue = this.Emitter.ReplaceAwaiterByVar;
            this.Emitter.ReplaceAwaiterByVar = true;
            string key = null;

            if (switchStatement.Expression is IdentifierExpression)
            {
                var oldBuilder = this.Emitter.Output;
                this.Emitter.Output = new StringBuilder();

                switchStatement.Expression.AcceptVisitor(this.Emitter);
                key = this.Emitter.Output.ToString().Trim();

                this.Emitter.Output = oldBuilder;
            }
            else
            {
                key = this.AddLocal(this.GetTempVarName(), AstType.Null);
                this.Write(key);
                this.Write(" = ");
                switchStatement.Expression.AcceptVisitor(this.Emitter);
                this.WriteSemiColon();
                this.WriteNewLine();
            }

            this.Emitter.ReplaceAwaiterByVar = oldValue;

            var list = switchStatement.SwitchSections.ToList();
            list.Sort((s1, s2) =>
            {
                var lbl = s1.CaseLabels.FirstOrDefault(l => l.Expression.IsNull);

                if (lbl != null)
                {
                    return 1;
                }

                lbl = s2.CaseLabels.FirstOrDefault(l => l.Expression.IsNull);

                if (lbl != null)
                {
                    return -1;
                }

                return 0;
            });

            var jumpStatements = this.Emitter.JumpStatements;
            this.Emitter.JumpStatements = new List<IJumpInfo>();
            bool writeElse = false;
            var thisStep = this.Emitter.AsyncBlock.Steps.Last();

            foreach (var switchSection in list)
            {
                this.VisitAsyncSwitchSection(switchSection, writeElse, key);
                writeElse = true;
            }

            var nextStep = this.Emitter.AsyncBlock.AddAsyncStep();
            thisStep.JumpToStep = nextStep.Step;

            if (this.Emitter.JumpStatements.Count > 0)
            {
                this.Emitter.JumpStatements.Sort((j1, j2) => -j1.Position.CompareTo(j2.Position));
                foreach (var jump in this.Emitter.JumpStatements)
                {
                    if (jump.Break)
                    {
                        jump.Output.Insert(jump.Position, nextStep.Step);
                    }
                    else if (jumpStatements != null)
                    {
                        jumpStatements.Add(jump);
                    }
                }
            }

            this.Emitter.JumpStatements = jumpStatements;
            this.Emitter.AsyncSwitch = this.ParentAsyncSwitch;
        }

        protected void VisitAsyncSwitchSection(SwitchSection switchSection, bool writeElse, string switchKey)
        {
            var list = switchSection.CaseLabels.ToList();

            list.Sort((l1, l2) =>
            {
                if (l1.Expression.IsNull)
                {
                    return 1;
                }

                if (l2.Expression.IsNull)
                {
                    return -1;
                }

                return 0;
            });

            if (writeElse)
            {
                this.WriteElse();
            }

            if (list.Any(l => l.Expression.IsNull))
            {
                if (!writeElse)
                {
                    this.WriteElse();
                }
            }
            else
            {
                this.WriteIf();
                this.WriteOpenParentheses();

                var oldValue = this.Emitter.ReplaceAwaiterByVar;
                this.Emitter.ReplaceAwaiterByVar = true;
                bool writeOr = false;

                foreach (var label in list)
                {
                    if (writeOr)
                    {
                        this.WriteSpace();
                        this.Write("||");
                        this.WriteSpace();
                    }

                    this.Write(switchKey + " === ");
                    label.Expression.AcceptVisitor(this.Emitter);

                    writeOr = true;
                }

                this.WriteCloseParentheses();
                this.Emitter.ReplaceAwaiterByVar = oldValue;
            }

            if (switchSection.Statements.Count() == 1 && switchSection.Statements.First() is BlockStatement)
            {
                this.Emitter.IgnoreBlock = switchSection.Statements.First();
            }

            int startCount = this.Emitter.AsyncBlock.Steps.Count;
            IAsyncStep thisStep = null;
            this.WriteSpace();
            this.BeginBlock();
            this.Write("$step = " + this.Emitter.AsyncBlock.Step + ";");
            this.WriteNewLine();
            this.Write("continue;");
            var writer = this.SaveWriter();
            var bodyStep = this.Emitter.AsyncBlock.AddAsyncStep();

            switchSection.Statements.AcceptVisitor(this.Emitter);

            if (this.Emitter.AsyncBlock.Steps.Count > startCount)
            {
                thisStep = this.Emitter.AsyncBlock.Steps.Last();
            }

            if (this.RestoreWriter(writer) && !this.IsOnlyWhitespaceOnPenultimateLine(true))
            {
                this.WriteNewLine();
            }

            this.EndBlock();
            this.WriteNewLine();
        }

        protected void VisitSwitchStatement() {
            SwitchStatement switchStatement = this.SwitchStatement;

            this.BeginDoBlock();

            string varName = this.GetTempVarName();

            this.WriteVar();
            this.Write(varName, " = ");
            switchStatement.Expression.AcceptVisitor(this.Emitter);
            this.WriteSemiColon(true);

            int index = 0;
            foreach(var section in switchStatement.SwitchSections) {
                new SwitchBlock(this.Emitter, section, varName,  index == 0, index == switchStatement.SwitchSections.Count - 1).Emit();
                index++;
            }

            RemoveTempVar(varName);

            this.EndCodeBlock();
            this.WriteNewLine();
        }

        protected void VisitSwitchSection() {
            SwitchSection switchSection = this.SwitchSection;

            bool isElse = false;

            if(isFirst_) {
                this.WriteIf();
            }
            else {
                if(switchSection.CaseLabels.Any(i => !i.Expression.IsNull)) {
                    this.Write("elseif ");
                }
                else {
                    this.WriteElse();
                    isElse = true;
                }
            }

            int index = 0;
            foreach(var caseLabel in switchSection.CaseLabels) {
                new SwitchBlock(this.Emitter, caseLabel, varName_, index == 0).Emit();
                ++index;
            }
            this.WriteSpace();
            if(!isElse) {
                this.BeginIfBlock();
            }
            else {
                this.WriteNewLine();
                this.Indent();
            }

            //var children = switchSection.Children.Where(c => c.Role == Roles.EmbeddedStatement || c.Role == Roles.Comment);
            foreach(var node in switchSection.Statements) {
                if(!(node is BreakStatement)) {
                    if(node is BlockStatement) {
                        var breakNodel = node.Children.FirstOrDefault(i => i is BreakStatement);
                        if(breakNodel != null) {
                            breakNodel.Remove();
                        }
                    }
                    node.AcceptVisitor(this.Emitter);
                }
            }

            if(isEnd_) {
                this.EndCodeBlock();
                this.WriteNewLine();
            }
            else {
                this.Outdent();
            }
        }

        protected void VisitCaseLabel() {
            CaseLabel caseLabel = this.CaseLabel;
            if(!caseLabel.Expression.IsNull) {
                if(!isFirst_) {
                    this.Write(" or ");
                }
                this.Write(varName_, " == ");
                caseLabel.Expression.AcceptVisitor(this.Emitter);
            }
        }

        /*
        protected void VisitSwitchStatement()
        {
            SwitchStatement switchStatement = this.SwitchStatement;

            this.WriteSwitch();
            this.WriteOpenParentheses();

            switchStatement.Expression.AcceptVisitor(this.Emitter);

            this.WriteCloseParentheses();
            this.WriteSpace();

            this.BeginBlock();
            switchStatement.SwitchSections.ToList().ForEach(s => s.AcceptVisitor(this.Emitter));
            this.EndBlock();
            this.WriteNewLine();
        }

        protected void VisitSwitchSection()
        {
            SwitchSection switchSection = this.SwitchSection;
            foreach(var caseLabel in switchSection.CaseLabels) {
                new SwitchBlock(this.Emitter, caseLabel).Emit();
            }
            this.Indent();

            var children = switchSection.Children.Where(c => c.Role == Roles.EmbeddedStatement || c.Role == Roles.Comment);
            children.ToList().ForEach(s => s.AcceptVisitor(this.Emitter));
            this.Outdent();
        }

        protected void VisitCaseLabel()
        {
            CaseLabel caseLabel = this.CaseLabel;
            if (caseLabel.Expression.IsNull)
            {
                this.Write("default");
            }
            else
            {
                this.Write("case ");
                caseLabel.Expression.AcceptVisitor(this.Emitter);
            }

            this.WriteEqualsSign();
            this.WriteNewLine();
        }*/
    }
}
