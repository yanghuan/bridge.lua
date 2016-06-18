using ICSharpCode.NRefactory.CSharp;
using System.Collections.Generic;

namespace Bridge.Translator
{
    public class LambdaVisitor : DepthFirstAstVisitor
    {
        public LambdaVisitor()
        {
            this.LambdaExpression = new List<Expression>();
        }

        public List<Expression> LambdaExpression
        {
            get;
            set;
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
        {
            this.LambdaExpression.Add(anonymousMethodExpression);
            base.VisitAnonymousMethodExpression(anonymousMethodExpression);
        }

        public override void VisitLambdaExpression(LambdaExpression lambdaExpression)
        {
            this.LambdaExpression.Add(lambdaExpression);
            base.VisitLambdaExpression(lambdaExpression);
        }
    }

    public class ContinueBreakVisitor : DepthFirstAstVisitor
    {
        private bool findReturn;
        public ContinueBreakVisitor(bool findReturn)
        {
            this.findReturn = findReturn;

            if (findReturn)
            {
                this.Return = new List<ReturnStatement>();
            }
            else
            {
                this.Continue = new List<ContinueStatement>();
                this.Break = new List<BreakStatement>();    
            }
        }

        public List<ContinueStatement> Continue
        {
            get;
            set;
        }

        public List<BreakStatement> Break
        {
            get;
            set;
        }

        public List<ReturnStatement> Return
        {
            get;
            set;
        }

        public bool InsideSwitch
        {
            get; set;
        }

        public override void VisitLambdaExpression(LambdaExpression lambdaExpression)
        {
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
        {
        }

        public override void VisitForStatement(ForStatement forStatement)
        {
            if (findReturn)
            {
                base.VisitForStatement(forStatement);
            }
        }

        public override void VisitForeachStatement(ForeachStatement foreachStatement)
        {
            if (findReturn)
            {
                base.VisitForeachStatement(foreachStatement);
            }
        }

        public override void VisitWhileStatement(WhileStatement whileStatement)
        {
            if (findReturn)
            {
                base.VisitWhileStatement(whileStatement);
            }
        }

        public override void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            if (findReturn)
            {
                base.VisitDoWhileStatement(doWhileStatement);
            }
        }

        public override void VisitContinueStatement(ContinueStatement continueStatement)
        {
            if (!findReturn)
            {
                this.Continue.Add(continueStatement);
            }
            base.VisitContinueStatement(continueStatement);
        }

        public override void VisitBreakStatement(BreakStatement breakStatement)
        {
            if (!findReturn && !this.InsideSwitch)
            {
                this.Break.Add(breakStatement);    
            }
            
            base.VisitBreakStatement(breakStatement);
        }

        public override void VisitReturnStatement(ReturnStatement returnStatement)
        {
            if (findReturn)
            {
                this.Return.Add(returnStatement);
            }
            base.VisitReturnStatement(returnStatement);
        }

        public override void VisitSwitchStatement(SwitchStatement switchStatement)
        {
            var oldValue = this.InsideSwitch;
            this.InsideSwitch = true;
            base.VisitSwitchStatement(switchStatement);
            this.InsideSwitch = oldValue;
        }
    }
}
