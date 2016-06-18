using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bridge.Translator
{
    public class AwaitSearchVisitor : DepthFirstAstVisitor
    {
        public AwaitSearchVisitor()
        {
            this.AwaitExpressions = new List<Tuple<int, Expression>>();
        }

        private List<Tuple<int, Expression>> AwaitExpressions
        {
            get;
            set;
        }

        public List<Expression> GetAwaitExpressions()
        {
            this.AwaitExpressions.Sort((t1, t2) => t2.Item1.CompareTo(t1.Item1));
            return this.AwaitExpressions.Select(t => t.Item2).ToList();
        }

        private int InvocationLevel
        {
            get;
            set;
        }

        public override void VisitLambdaExpression(LambdaExpression lambdaExpression)
        {
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
        {
        }

        public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        {
            this.InvocationLevel++;
            base.VisitInvocationExpression(invocationExpression);
            this.InvocationLevel--;
        }

        public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        {
            if (unaryOperatorExpression.Operator == UnaryOperatorType.Await)
            {
                this.AwaitExpressions.Add(new Tuple<int, Expression>(this.InvocationLevel, unaryOperatorExpression.Expression));
            }

            base.VisitUnaryOperatorExpression(unaryOperatorExpression);
        }
    }

    public class AsyncTryVisitor : DepthFirstAstVisitor
    {
        public AsyncTryVisitor()
        {
        }

        public bool Found
        {
            get;
            set;
        }

        public override void VisitLambdaExpression(LambdaExpression lambdaExpression)
        {
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
        {
        }

        public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        {
            if (unaryOperatorExpression.Operator == UnaryOperatorType.Await)
            {
                var tryBlock = unaryOperatorExpression.GetParent<TryCatchStatement>();

                if (tryBlock != null)
                {
                    this.Found = true;
                }
            }

            base.VisitUnaryOperatorExpression(unaryOperatorExpression);
        }
    }
}
