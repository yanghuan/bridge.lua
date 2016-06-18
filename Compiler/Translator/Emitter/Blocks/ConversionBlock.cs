using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System.Collections.Generic;
using System.Linq;

namespace Bridge.Translator
{
    public abstract class ConversionBlock : AbstractEmitterBlock
    {
        public ConversionBlock(IEmitter emitter, AstNode node)
            : base(emitter, node)
        {
        }

        protected sealed override void DoEmit()
        {
            var expression = this.GetExpression();

            if (expressionInWork.Contains(expression))
            {
                this.EmitConversionExpression();
                return;
            }

            expressionInWork.Add(expression);

            int parenthesesLevel = 0;
            bool check = expression != null && !expression.IsNull && expression.Parent != null;

            if (check)
            {
                parenthesesLevel = this.CheckConversion(expression);
            }

            if (this.DisableEmitConversionExpression)
            {
                expressionInWork.Remove(expression);
                return;
            }

            this.EmitConversionExpression();
            expressionInWork.Remove(expression);

            if (parenthesesLevel > 0)
            {
                for (int i = 0; i < parenthesesLevel; i++)
                {
                    this.WriteCloseParentheses();
                }
            }
        }

        private static List<Expression> expressionInWork = new List<Expression>();

        protected virtual bool DisableEmitConversionExpression
        {
            get;
            set;
        }

        protected virtual int CheckConversion(Expression expression)
        {
            return ConversionBlock.CheckConversion(this, expression);
        }

        public static bool IsUserDefinedConversion(AbstractEmitterBlock block, Expression expression)
        {
            Conversion conversion = null;

            try
            {
                var rr = block.Emitter.Resolver.ResolveNode(expression, null);
                conversion = block.Emitter.Resolver.Resolver.GetConversion(expression);

                if (conversion == null)
                {
                    return false;
                }

                return conversion.IsUserDefined;
            }
            catch
            {
            }

            return false;
        }

        public static int CheckConversion(ConversionBlock block, Expression expression)
        {
            try
            {
                var rr = block.Emitter.Resolver.ResolveNode(expression, block.Emitter);
                var conversion = block.Emitter.Resolver.Resolver.GetConversion(expression);
                var expectedType = block.Emitter.Resolver.Resolver.GetExpectedType(expression);

                var level = ConversionBlock.CheckDecimalConversion(block, expression, rr, expectedType, conversion) ? 1 : 0;

                if (Helpers.IsDecimalType(expectedType, block.Emitter.Resolver) && !conversion.IsUserDefined)
                {
                    return level;
                }

                if (conversion == null)
                {
                    return level;
                }

                if (conversion.IsIdentityConversion)
                {
                    return level;
                }

                var isNumLifted = conversion.IsImplicit && conversion.IsLifted && conversion.IsNumericConversion && !(expression is BinaryOperatorExpression);
                if (isNumLifted && !conversion.IsUserDefined)
                {
                    return level;
                }
                bool isLifted = conversion.IsLifted && !isNumLifted && !(block is CastBlock) && !Helpers.IsDecimalType(expectedType, block.Emitter.Resolver);
                if (isLifted)
                {
                    level++;
                    block.Write("Bridge.Nullable.lift(");
                }

                if (conversion.IsUserDefined)
                {
                    var method = conversion.Method;

                    string inline = block.Emitter.GetInline(method);

                    if (conversion.IsExplicit && !string.IsNullOrWhiteSpace(inline))
                    {
                        // Still returns true if Nullable.lift( was written.
                        return level;
                    }

                    if (!string.IsNullOrWhiteSpace(inline))
                    {
                        if (expression is InvocationExpression)
                        {
                            new InlineArgumentsBlock(block.Emitter, new ArgumentsInfo(block.Emitter, (InvocationExpression)expression), inline).Emit();
                        }
                        else if (expression is ObjectCreateExpression)
                        {
                            new InlineArgumentsBlock(block.Emitter, new ArgumentsInfo(block.Emitter, (InvocationExpression)expression), inline).Emit();
                        }
                        else if (expression is UnaryOperatorExpression)
                        {
                            var unaryExpression = (UnaryOperatorExpression)expression;
                            var resolveOperator = block.Emitter.Resolver.ResolveNode(unaryExpression, block.Emitter);
                            OperatorResolveResult orr = resolveOperator as OperatorResolveResult;
                            new InlineArgumentsBlock(block.Emitter, new ArgumentsInfo(block.Emitter, unaryExpression, orr, method), inline).Emit();
                        }
                        else if (expression is BinaryOperatorExpression)
                        {
                            var binaryExpression = (BinaryOperatorExpression)expression;
                            var resolveOperator = block.Emitter.Resolver.ResolveNode(binaryExpression, block.Emitter);
                            OperatorResolveResult orr = resolveOperator as OperatorResolveResult;
                            new InlineArgumentsBlock(block.Emitter, new ArgumentsInfo(block.Emitter, binaryExpression, orr, method), inline).Emit();
                        }
                        else
                        {
                            new InlineArgumentsBlock(block.Emitter, new ArgumentsInfo(block.Emitter, expression), inline).Emit();
                        }

                        block.DisableEmitConversionExpression = true;

                        // Still returns true if Nullable.lift( was written.
                        return level;
                    }
                    else
                    {
                        if (method.DeclaringTypeDefinition != null && block.Emitter.Validator.IsIgnoreType(method.DeclaringTypeDefinition))
                        {
                            // Still returns true if Nullable.lift( was written.
                            return level;
                        }

                        block.Write(BridgeTypes.ToJsName(method.DeclaringType, block.Emitter));
                        block.WriteDot();

                        block.Write(OverloadsCollection.Create(block.Emitter, method).GetOverloadName());
                    }

                    if (isLifted)
                    {
                        block.WriteComma();
                    }
                    else
                    {
                        block.WriteOpenParentheses();
                        level++;
                    }

                    var arg = method.Parameters[0];

                    if (Helpers.IsDecimalType(arg.Type, block.Emitter.Resolver, arg.IsParams) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver) && !expression.IsNull)
                    {
                        block.Write("Bridge.Decimal");
                        if (NullableType.IsNullable(arg.Type) && ConversionBlock.ShouldBeLifted(expression))
                        {
                            block.Write(".lift");
                        }
                        block.WriteOpenParentheses();
                        level++;
                    }

                    return level;
                }
                // Still returns true if Nullable.lift( was written.
                return level;
            }
            catch
            {
            }

            return 0;
        }

        private static bool CheckDecimalConversion(ConversionBlock block, Expression expression, ResolveResult rr, IType expectedType, Conversion conversion)
        {
            if (conversion.IsUserDefined)
            {
                var m = conversion.Method;
                if (Helpers.IsDecimalType(m.ReturnType, block.Emitter.Resolver))
                {
                    return false;
                }
            }

            var invocationExpression = expression.Parent as InvocationExpression;
            if (invocationExpression != null && invocationExpression.Arguments.Any(a => a == expression))
            {
                var index = invocationExpression.Arguments.ToList().IndexOf(expression);
                var methodResolveResult = block.Emitter.Resolver.ResolveNode(invocationExpression, block.Emitter) as MemberResolveResult;

                if (methodResolveResult != null)
                {
                    var m = methodResolveResult.Member as IMethod;
                    var arg = m.Parameters[index < m.Parameters.Count ? index : (m.Parameters.Count - 1)];

                    if (Helpers.IsDecimalType(arg.Type, block.Emitter.Resolver, arg.IsParams) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver))
                    {
                        if (expression.IsNull)
                        {
                            return false;
                        }

                        block.Write("Bridge.Decimal");
                        if (NullableType.IsNullable(arg.Type) && ConversionBlock.ShouldBeLifted(expression))
                        {
                            block.Write(".lift");
                        }
                        block.WriteOpenParentheses();
                        return true;
                    }
                }
            }

            var namedArgExpression = expression.Parent as NamedArgumentExpression;
            if (namedArgExpression != null)
            {
                var namedArgResolveResult = block.Emitter.Resolver.ResolveNode(namedArgExpression, block.Emitter) as NamedArgumentResolveResult;

                if (Helpers.IsDecimalType(namedArgResolveResult.Type, block.Emitter.Resolver) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write("Bridge.Decimal");
                    if (NullableType.IsNullable(namedArgResolveResult.Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write(".lift");
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var namedExpression = expression.Parent as NamedExpression;
            if (namedExpression != null)
            {
                var namedResolveResult = block.Emitter.Resolver.ResolveNode(namedExpression, block.Emitter);

                if (Helpers.IsDecimalType(namedResolveResult.Type, block.Emitter.Resolver) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write("Bridge.Decimal");
                    if (NullableType.IsNullable(namedResolveResult.Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write(".lift");
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var binaryOpExpr = expression.Parent as BinaryOperatorExpression;
            if (binaryOpExpr != null)
            {
                var idx = binaryOpExpr.Left == expression ? 0 : 1;
                var binaryOpRr = block.Emitter.Resolver.ResolveNode(binaryOpExpr, block.Emitter) as OperatorResolveResult;

                if (binaryOpRr != null && Helpers.IsDecimalType(binaryOpRr.Operands[idx].Type, block.Emitter.Resolver) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write("Bridge.Decimal");
                    if (NullableType.IsNullable(binaryOpRr.Operands[idx].Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write(".lift");
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var conditionalExpr = expression.Parent as ConditionalExpression;
            if (conditionalExpr != null && conditionalExpr.Condition != expression)
            {
                var idx = conditionalExpr.TrueExpression == expression ? 0 : 1;
                var conditionalrr = block.Emitter.Resolver.ResolveNode(conditionalExpr, block.Emitter) as OperatorResolveResult;

                if (conditionalrr != null && Helpers.IsDecimalType(conditionalrr.Operands[idx].Type, block.Emitter.Resolver) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write("Bridge.Decimal");
                    if (NullableType.IsNullable(conditionalrr.Operands[idx].Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write(".lift");
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var assignmentExpr = expression.Parent as AssignmentExpression;
            if (assignmentExpr != null)
            {
                var assigmentRr = block.Emitter.Resolver.ResolveNode(assignmentExpr, block.Emitter) as OperatorResolveResult;

                if (Helpers.IsDecimalType(assigmentRr.Operands[1].Type, block.Emitter.Resolver) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write("Bridge.Decimal");
                    if (NullableType.IsNullable(assigmentRr.Operands[1].Type) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write(".lift");
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            var arrayInit = expression.Parent as ArrayInitializerExpression;
            if (arrayInit != null)
            {
                while (arrayInit.Parent is ArrayInitializerExpression)
                {
                    arrayInit = (ArrayInitializerExpression)arrayInit.Parent;
                }

                IType elementType = null;
                var arrayCreate = arrayInit.Parent as ArrayCreateExpression;
                if (arrayCreate != null)
                {
                    var rrArrayType = block.Emitter.Resolver.ResolveNode(arrayCreate, block.Emitter);
                    if (rrArrayType.Type is TypeWithElementType)
                    {
                        elementType = ((TypeWithElementType)rrArrayType.Type).ElementType;
                    }
                    else
                    {
                        elementType = rrArrayType.Type;
                    }
                }
                else
                {
                    var rrElemenet = block.Emitter.Resolver.ResolveNode(arrayInit.Parent, block.Emitter);
                    var pt = rrElemenet.Type as ParameterizedType;
                    if (pt != null)
                    {
                        elementType = pt.TypeArguments.Count > 0 ? pt.TypeArguments.First() : null;
                    }
                    else
                    {
                        var arrayType = rrElemenet.Type as TypeWithElementType;
                        if (arrayType != null)
                        {
                            elementType = arrayType.ElementType;
                        }
                        else
                        {
                            elementType = rrElemenet.Type;
                        }
                    }
                }

                if (elementType != null && Helpers.IsDecimalType(elementType, block.Emitter.Resolver) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write("Bridge.Decimal");
                    if (NullableType.IsNullable(elementType) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write(".lift");
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            if (Helpers.IsDecimalType(expectedType, block.Emitter.Resolver) && !Helpers.IsDecimalType(rr.Type, block.Emitter.Resolver))
            {
                var castExpr = expression.Parent as CastExpression;
                ResolveResult castTypeRr = null;
                if (castExpr != null)
                {
                    castTypeRr = block.Emitter.Resolver.ResolveNode(castExpr.Type, block.Emitter);
                }

                if (castTypeRr == null || !Helpers.IsDecimalType(castTypeRr.Type, block.Emitter.Resolver))
                {
                    if (expression.IsNull)
                    {
                        return false;
                    }

                    block.Write("Bridge.Decimal");
                    if (NullableType.IsNullable(expectedType) && ConversionBlock.ShouldBeLifted(expression))
                    {
                        block.Write(".lift");
                    }
                    block.WriteOpenParentheses();
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldBeLifted(Expression expr)
        {
            return !(expr is PrimitiveExpression || expr.IsNull);
        }

        protected abstract void EmitConversionExpression();

        protected abstract Expression GetExpression();
    }
}
