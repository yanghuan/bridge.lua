using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using System.Collections.Generic;

namespace Bridge.Translator
{
    public abstract class AbstractObjectCreateBlock : AbstractEmitterBlock
    {
        public AbstractObjectCreateBlock(IEmitter emitter, AstNode node)
            : base(emitter, node)
        {
        }

        protected virtual void WriteObjectInitializer(IEnumerable<Expression> expressions, bool changeCase)
        {
            bool needComma = false;

            foreach (Expression item in expressions)
            {
                NamedExpression namedExression = null;
                NamedArgumentExpression namedArgumentExpression = null;
                IdentifierExpression identifierExpression = null;
                MemberReferenceExpression memberReferenceExpression = null;

                namedExression = item as NamedExpression;
                if (namedExression == null)
                {
                    namedArgumentExpression = item as NamedArgumentExpression;

                    if (namedArgumentExpression == null)
                    {
                        identifierExpression = item as IdentifierExpression;
                        if (identifierExpression == null)
                        {
                            memberReferenceExpression = item as MemberReferenceExpression;
                        }
                    }
                }

                if (needComma)
                {
                    this.WriteComma();
                }

                needComma = true;
                string name;
                Expression expression;

                if (namedExression != null)
                {
                    name = namedExression.Name;
                    expression = namedExression.Expression;
                }
                else if (namedArgumentExpression != null)
                {
                    name = namedArgumentExpression.Name;
                    expression = namedArgumentExpression.Expression;
                }
                else if (identifierExpression != null)
                {
                    name = identifierExpression.Identifier;
                    expression = identifierExpression;
                }
                else
                {
                    name = memberReferenceExpression.MemberName;
                    expression = memberReferenceExpression;
                }

                if (changeCase)
                {
                    name = Object.Net.Utilities.StringUtils.ToLowerCamelCase(name);
                }

                this.Write(name, ": ");
                expression.AcceptVisitor(this.Emitter);
            }
        }
    }
}
