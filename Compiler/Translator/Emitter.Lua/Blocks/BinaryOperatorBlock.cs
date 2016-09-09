using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Linq;

namespace Bridge.Translator.Lua
{
    public class BinaryOperatorBlock : ConversionBlock
    {
        public BinaryOperatorBlock(IEmitter emitter, BinaryOperatorExpression binaryOperatorExpression)
            : base(emitter, binaryOperatorExpression)
        {
            this.Emitter = emitter;
            this.BinaryOperatorExpression = binaryOperatorExpression;
        }

        public BinaryOperatorExpression BinaryOperatorExpression
        {
            get;
            set;
        }

        protected override Expression GetExpression()
        {
            return this.BinaryOperatorExpression;
        }

        protected override void EmitConversionExpression()
        {
            this.VisitBinaryOperatorExpression();
        }

        protected bool ResolveOperator(BinaryOperatorExpression binaryOperatorExpression, OperatorResolveResult orr)
        {
            var method = orr != null ? orr.UserDefinedOperatorMethod : null;

            if (method != null)
            {
                var inline = this.Emitter.GetInline(method);

                if (!string.IsNullOrWhiteSpace(inline))
                {
                    new InlineArgumentsBlock(this.Emitter,
                        new ArgumentsInfo(this.Emitter, binaryOperatorExpression, orr, method), inline).Emit();
                    return true;
                }
                else if (!this.Emitter.Validator.IsIgnoreType(method.DeclaringTypeDefinition))
                {
                    string name = OverloadsCollection.Create(this.Emitter, method).GetOverloadName();
                    if(Helpers.GetOperatorMapping(name) != null) {
                        return false;
                    }

                    if (orr.IsLiftedOperator)
                    {
                        this.Write(Bridge.Translator.Emitter.ROOT + ".Nullable.");

                        string action = "lift";

                        switch (this.BinaryOperatorExpression.Operator)
                        {
                            case BinaryOperatorType.GreaterThan:
                                action = "liftcmp";
                                break;

                            case BinaryOperatorType.GreaterThanOrEqual:
                                action = "liftcmp";
                                break;

                            case BinaryOperatorType.Equality:
                                action = "lifteq";
                                break;

                            case BinaryOperatorType.InEquality:
                                action = "liftne";
                                break;

                            case BinaryOperatorType.LessThan:
                                action = "liftcmp";
                                break;

                            case BinaryOperatorType.LessThanOrEqual:
                                action = "liftcmp";
                                break;
                        }

                        this.Write(action + "(");
                    }

                    this.Write(BridgeTypes.ToJsName(method.DeclaringType, this.Emitter));
                    this.WriteDot();
                    this.Write(name);

                    if (orr.IsLiftedOperator)
                    {
                        this.WriteComma();
                    }
                    else
                    {
                        this.WriteOpenParentheses();
                    }

                    new ExpressionListBlock(this.Emitter, new Expression[] { binaryOperatorExpression.Left, binaryOperatorExpression.Right }, null).Emit();
                    this.WriteCloseParentheses();
                    return true;
                }
            }

            return false;
        }

        private object WrapConstValue(object v) {
            if(v is string) {
                v = "\"" + v + "\"";
            }
            return v;
        }

        private object GetConstValue(OperatorResolveResult orr, bool isLeft, bool isChar) {
            var result = isLeft ? orr.Operands.First() : orr.Operands.Last();
            if(result.IsCompileTimeConstant) {
                return WrapConstValue(result.ConstantValue);
            }
            ConversionResolveResult rr = result as ConversionResolveResult;
            if(rr != null && rr.Input.IsCompileTimeConstant) {
                if(isChar) {
                    return isLeft ? this.BinaryOperatorExpression.Left.ToString() : this.BinaryOperatorExpression.Right.ToString();
                }
                else if(rr.Input.Type.Kind == TypeKind.Enum) {
                    MemberResolveResult reslut = (MemberResolveResult)rr.Input;
                    return "\"" + reslut.Member.Name + "\"";
                }
                return WrapConstValue(rr.Input.ConstantValue);
            }
            return null;
        }

        private void VisitStringConcat(OperatorResolveResult orr, bool isLeft, bool isChar) {
            object constValue = GetConstValue(orr, isLeft, isChar);
            if(constValue != null) {
                this.Write(constValue);
            }
            else {
                var express = isLeft ? this.BinaryOperatorExpression.Left : this.BinaryOperatorExpression.Right;
                if(isChar) {
                    this.Write("string.char");
                    this.WriteOpenParentheses();
                    express.AcceptVisitor(this.Emitter);
                    this.WriteCloseParentheses();
                }
                else if(express is IdentifierExpression || express is InvocationExpression) {
                    var resolverResult = this.Emitter.Resolver.ResolveNode(express, this.Emitter);
                    if(resolverResult.Type.Kind == TypeKind.Struct) {
                        express.AcceptVisitor(this.Emitter);
                    }
                    else if(resolverResult.Type.Kind == TypeKind.Enum) {
                        TransformCtx.ExportEnums.Add(resolverResult.Type);
                        this.Write("System.Enum.toString");
                        this.WriteOpenParentheses();
                        express.AcceptVisitor(this.Emitter);
                        this.WriteComma();
                        string typeName = BridgeTypes.ToJsName(resolverResult.Type, this.Emitter);
                        this.Write(typeName);
                        this.WriteCloseParentheses();
                    }
                    else {
                        this.Write("System.strconcat");
                        this.WriteOpenParentheses();
                        express.AcceptVisitor(this.Emitter);
                        this.WriteCloseParentheses();
                    }
                }
                else {
                    express.AcceptVisitor(this.Emitter);
                }
            }
        }

        protected void VisitBinaryOperatorExpression()
        {
            BinaryOperatorExpression binaryOperatorExpression = this.BinaryOperatorExpression;
            var resolveOperator = this.Emitter.Resolver.ResolveNode(binaryOperatorExpression, this.Emitter);
            var expectedType = this.Emitter.Resolver.Resolver.GetExpectedType(binaryOperatorExpression);
            bool isDecimalExpected = Helpers.IsDecimalType(expectedType, this.Emitter.Resolver);
            bool isDecimal = Helpers.IsDecimalType(resolveOperator.Type, this.Emitter.Resolver);
            OperatorResolveResult orr = resolveOperator as OperatorResolveResult;
            var leftResolverResult = this.Emitter.Resolver.ResolveNode(binaryOperatorExpression.Left, this.Emitter);
            var rightResolverResult = this.Emitter.Resolver.ResolveNode(binaryOperatorExpression.Right, this.Emitter);
            var charToString = -1;
            string variable = null;
            bool leftIsNull = this.BinaryOperatorExpression.Left is NullReferenceExpression;
            bool rightIsNull = this.BinaryOperatorExpression.Right is NullReferenceExpression;
            bool isStringConcat = false;

            /*
            if ((leftIsNull || rightIsNull) && (binaryOperatorExpression.Operator == BinaryOperatorType.Equality || binaryOperatorExpression.Operator == BinaryOperatorType.InEquality))
            {
                if (binaryOperatorExpression.Operator == BinaryOperatorType.Equality)
                {
                    this.Write("!");
                }

                this.Write(LuaHelper.Root,".hasValue");
                
                this.WriteOpenParentheses();
                
                if (leftIsNull)
                {
                    binaryOperatorExpression.Right.AcceptVisitor(this.Emitter);
                }
                else
                {
                    binaryOperatorExpression.Left.AcceptVisitor(this.Emitter);
                }
                
                this.WriteCloseParentheses();
                return;
            }*/

            if (orr != null && orr.Type.IsKnownType(KnownTypeCode.String))
            {
                isStringConcat = true;
                for (int i = 0; i < orr.Operands.Count; i++)
                {
                    var crr = orr.Operands[i] as ConversionResolveResult;
                    if (crr != null && crr.Input.Type.IsKnownType(KnownTypeCode.Char))
                    {
                        charToString = i;
                    }
                }
            }

            if (resolveOperator is ConstantResolveResult)
            {
                this.WriteScript(((ConstantResolveResult)resolveOperator).ConstantValue);
                return;
            }

            if (!((expectedType.IsKnownType(KnownTypeCode.String) || resolveOperator.Type.IsKnownType(KnownTypeCode.String)) && binaryOperatorExpression.Operator == BinaryOperatorType.Add) && (Helpers.IsDecimalType(leftResolverResult.Type, this.Emitter.Resolver) || Helpers.IsDecimalType(rightResolverResult.Type, this.Emitter.Resolver)))
            {
                isDecimal = true;
                isDecimalExpected = true;
            }

            if (isDecimal && isDecimalExpected && binaryOperatorExpression.Operator != BinaryOperatorType.NullCoalescing)
            {
                this.HandleDecimal(resolveOperator);
                return;
            }

            var delegateOperator = false;

            if (this.ResolveOperator(binaryOperatorExpression, orr))
            {
                return;
            }

            /*
            if (binaryOperatorExpression.Operator == BinaryOperatorType.Divide &&
                (
                    (Helpers.IsIntegerType(leftResolverResult.Type, this.Emitter.Resolver) &&
                    Helpers.IsIntegerType(rightResolverResult.Type, this.Emitter.Resolver)) ||

                    (Helpers.IsIntegerType(this.Emitter.Resolver.Resolver.GetExpectedType(binaryOperatorExpression.Left), this.Emitter.Resolver) &&
                    Helpers.IsIntegerType(this.Emitter.Resolver.Resolver.GetExpectedType(binaryOperatorExpression.Right), this.Emitter.Resolver))
                ))
            {
                this.Write("{0}.Number.div(".F(LuaHelper.Root));
                binaryOperatorExpression.Left.AcceptVisitor(this.Emitter);
                this.Write(", ");
                binaryOperatorExpression.Right.AcceptVisitor(this.Emitter);
                this.Write(")");
                return;
            }
            */

            if (binaryOperatorExpression.Operator == BinaryOperatorType.Add ||
                binaryOperatorExpression.Operator == BinaryOperatorType.Subtract)
            {
                var add = binaryOperatorExpression.Operator == BinaryOperatorType.Add;
                if (this.Emitter.Validator.IsDelegateOrLambda(leftResolverResult) || this.Emitter.Validator.IsDelegateOrLambda(rightResolverResult))
                {
                    delegateOperator = true;
                    this.Write(Bridge.Translator.Emitter.ROOT + "." + (add ? Bridge.Translator.Emitter.DELEGATE_COMBINE : Bridge.Translator.Emitter.DELEGATE_REMOVE));
                    this.WriteOpenParentheses();
                }
            }

            bool isBool = NullableType.IsNullable(resolveOperator.Type) ? NullableType.GetUnderlyingType(resolveOperator.Type).IsKnownType(KnownTypeCode.Boolean) : resolveOperator.Type.IsKnownType(KnownTypeCode.Boolean);
            bool isBitwise = (binaryOperatorExpression.Operator == BinaryOperatorType.BitwiseAnd && !isBool) ||
                             (binaryOperatorExpression.Operator == BinaryOperatorType.BitwiseOr &&  !isBool) ||
                             binaryOperatorExpression.Operator == BinaryOperatorType.ExclusiveOr ||
                             binaryOperatorExpression.Operator == BinaryOperatorType.ShiftLeft ||
                             binaryOperatorExpression.Operator == BinaryOperatorType.ShiftRight;

            bool isIntDiv = binaryOperatorExpression.Operator == BinaryOperatorType.Divide &&
                (
                    (Helpers.IsIntegerType(leftResolverResult.Type, this.Emitter.Resolver) &&
                    Helpers.IsIntegerType(rightResolverResult.Type, this.Emitter.Resolver)) ||

                    (Helpers.IsIntegerType(this.Emitter.Resolver.Resolver.GetExpectedType(binaryOperatorExpression.Left), this.Emitter.Resolver) &&
                    Helpers.IsIntegerType(this.Emitter.Resolver.Resolver.GetExpectedType(binaryOperatorExpression.Right), this.Emitter.Resolver))
                );

            bool nullable = orr != null && orr.IsLiftedOperator;
            bool isCoalescing = binaryOperatorExpression.Operator == BinaryOperatorType.NullCoalescing;
            string root = Bridge.Translator.Emitter.ROOT + ".Nullable.";
            bool special = nullable;
            bool rootSpecial = nullable;

            if(!nullable) {
                if(isBitwise || isIntDiv) {
                    root = Bridge.Translator.Emitter.ROOT + ".";
                    special = true;
                    rootSpecial = true;
                }
            }

            if (rootSpecial)
            {
                this.Write(root);
            }
            else if(isStringConcat) {
                VisitStringConcat(orr, true, charToString == 0);
            }
            else
            {
                if (isCoalescing)
                {
                    this.Write("(");
                    variable = this.GetTempVarName();
                    this.Write(variable);
                    this.Write(" = ");
                }
                else if (charToString == 0)
                {
                    this.Write("string.char(");
                }

                binaryOperatorExpression.Left.AcceptVisitor(this.Emitter);

                if (isCoalescing)
                {
                    this.Write(", {0}.hasValue(".F(LuaHelper.Root));
                    this.Write(variable);
                    this.Write(") ? ");
                    this.Write(variable);
                }
                else if (charToString == 0)
                {
                    this.Write(")");
                }
            }

            if (!delegateOperator)
            {
                if (!special)
                {
                    this.WriteSpace();
                }

                bool isUint = resolveOperator.Type.IsKnownType(KnownTypeCode.UInt16) ||
                              resolveOperator.Type.IsKnownType(KnownTypeCode.UInt32) ||
                              resolveOperator.Type.IsKnownType(KnownTypeCode.UInt64);

                bool is64bit = resolveOperator.Type.IsKnownType(KnownTypeCode.UInt64) ||
                               resolveOperator.Type.IsKnownType(KnownTypeCode.Int64);

                if (isBitwise && is64bit)
                {
                    throw new EmitterException(this.BinaryOperatorExpression, "Bitwise operations are not allowed on 64-bit types");
                }

                switch (binaryOperatorExpression.Operator)
                {
                    case BinaryOperatorType.Add:
                        if(isStringConcat) {
                            this.Write(rootSpecial ? "and" : "..");
                        }
                        else {
                            this.Write(rootSpecial ? "and" : "+");
                        }
                        break;

                    case BinaryOperatorType.BitwiseAnd:
                        if (isBool)
                        {
                            this.Write(rootSpecial ? "and" : "and");
                        }
                        else
                        {
                            this.Write(rootSpecial ? "band" : "&");
                        }

                        break;

                    case BinaryOperatorType.BitwiseOr:
                        if (isBool)
                        {
                            this.Write(rootSpecial ? "or" : "or");
                        }
                        else
                        {
                            this.Write(rootSpecial ? "bor" : "|");
                        }
                        break;

                    case BinaryOperatorType.ConditionalAnd:
                        this.Write(rootSpecial ? "and" : "and");
                        break;

                    case BinaryOperatorType.NullCoalescing:
                        this.Write(":");
                        break;

                    case BinaryOperatorType.ConditionalOr:
                        this.Write(rootSpecial ? "or" : "or");
                        break;

                    case BinaryOperatorType.Divide:
                        this.Write(rootSpecial ? "div" : "/");
                        break;

                    case BinaryOperatorType.Equality:
                        this.Write(rootSpecial ? "eq" : "==");
                        break;

                    case BinaryOperatorType.ExclusiveOr:
                        this.Write(rootSpecial ? "xor" : "^");
                        break;

                    case BinaryOperatorType.GreaterThan:
                        this.Write(rootSpecial ? ">" : ">");
                        break;

                    case BinaryOperatorType.GreaterThanOrEqual:
                        this.Write(rootSpecial ? "gte" : ">=");
                        break;

                    case BinaryOperatorType.InEquality:
                        this.Write(rootSpecial ? "neq" : "~=");
                        break;

                    case BinaryOperatorType.LessThan:
                        this.Write(rootSpecial ? "lt" : "<");
                        break;

                    case BinaryOperatorType.LessThanOrEqual:
                        this.Write(rootSpecial ? "lte" : "<=");
                        break;

                    case BinaryOperatorType.Modulus:
                        this.Write(rootSpecial ? "mod" : "%");
                        break;

                    case BinaryOperatorType.Multiply:
                        this.Write(rootSpecial ? "mul" : "*");
                        break;

                    case BinaryOperatorType.ShiftLeft:
                        this.Write(rootSpecial ? "sl" : "<<");
                        break;

                    case BinaryOperatorType.ShiftRight:
                        if (isUint)
                        {
                            this.Write(rootSpecial ? "srr" : ">>>");
                        }
                        else
                        {
                            this.Write(rootSpecial ? "sr" : ">>");
                        }

                        break;

                    case BinaryOperatorType.Subtract:
                        this.Write(rootSpecial ? "sub" : "-");
                        break;

                    default:
                        throw new EmitterException(binaryOperatorExpression, "Unsupported binary operator: " + binaryOperatorExpression.Operator.ToString());
                }
            }
            else
            {
                this.WriteComma();
            }

            if(isStringConcat) {
                this.WriteSpace();
                VisitStringConcat(orr, false, charToString == 1);
            }
            else {
                if(special) {
                    this.WriteOpenParentheses();
                    if(charToString == 0) {
                        this.Write("string.char(");
                    }

                    binaryOperatorExpression.Left.AcceptVisitor(this.Emitter);

                    if(charToString == 0) {
                        this.Write(")");
                    }

                    this.WriteComma();
                }
                else {
                    this.WriteSpace();
                }

                if(charToString == 1) {
                    this.Write("string.char(");
                }

                binaryOperatorExpression.Right.AcceptVisitor(this.Emitter);

                if(charToString == 1 || isCoalescing) {
                    this.Write(")");
                }

                if(delegateOperator || special) {
                    this.WriteCloseParentheses();
                }
            }
        }

        private void HandleDecimal(ResolveResult resolveOperator)
        {
            var orr = resolveOperator as OperatorResolveResult;
            var method = orr != null ? orr.UserDefinedOperatorMethod : null;

            if (orr != null && method == null)
            {
                var name = Helpers.GetBinaryOperatorMethodName(this.BinaryOperatorExpression.Operator);
                var type = this.Emitter.Resolver.Compilation.FindType(KnownTypeCode.Decimal);
                method = type.GetMethods(m => m.Name == name, GetMemberOptions.IgnoreInheritedMembers).FirstOrDefault();
            }

            if (method != null)
            {
                var inline = this.Emitter.GetInline(method);

                if (orr.IsLiftedOperator)
                {
                    this.Write(Bridge.Translator.Emitter.ROOT + ".Nullable.");
                    string action = "lift2";
                    string op_name = null;

                    switch (this.BinaryOperatorExpression.Operator)
                    {
                        case BinaryOperatorType.GreaterThan:
                            op_name = "gt";
                            action = "liftcmp";
                            break;

                        case BinaryOperatorType.GreaterThanOrEqual:
                            op_name = "gte";
                            action = "liftcmp";
                            break;

                        case BinaryOperatorType.Equality:
                            op_name = "equals";
                            action = "lifteq";
                            break;

                        case BinaryOperatorType.InEquality:
                            op_name = "ne";
                            action = "liftne";
                            break;

                        case BinaryOperatorType.LessThan:
                            op_name = "lt";
                            action = "liftcmp";
                            break;

                        case BinaryOperatorType.LessThanOrEqual:
                            op_name = "lte";
                            action = "liftcmp";
                            break;

                        case BinaryOperatorType.Add:
                            op_name = "add";
                            break;

                        case BinaryOperatorType.Subtract:
                            op_name = "sub";
                            break;

                        case BinaryOperatorType.Multiply:
                            op_name = "mul";
                            break;

                        case BinaryOperatorType.Divide:
                            op_name = "div";
                            break;

                        case BinaryOperatorType.Modulus:
                            op_name = "mod";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    this.Write(action);
                    this.WriteOpenParentheses();
                    this.WriteScript(op_name);
                    this.WriteComma();
                    new ExpressionListBlock(this.Emitter,
                        new Expression[] { this.BinaryOperatorExpression.Left, this.BinaryOperatorExpression.Right }, null).Emit();
                    this.WriteCloseParentheses();
                }
                else if (!string.IsNullOrWhiteSpace(inline))
                {
                    new InlineArgumentsBlock(this.Emitter,
                        new ArgumentsInfo(this.Emitter, this.BinaryOperatorExpression, orr, method), inline).Emit();
                }
                else if (!this.Emitter.Validator.IsIgnoreType(method.DeclaringTypeDefinition))
                {
                    this.Write(BridgeTypes.ToJsName(method.DeclaringType, this.Emitter));
                    this.WriteDot();

                    this.Write(OverloadsCollection.Create(this.Emitter, method).GetOverloadName());

                    this.WriteOpenParentheses();

                    new ExpressionListBlock(this.Emitter,
                        new Expression[] { this.BinaryOperatorExpression.Left, this.BinaryOperatorExpression.Right }, null).Emit();
                    this.WriteCloseParentheses();
                }
            }
        }
    }
}
