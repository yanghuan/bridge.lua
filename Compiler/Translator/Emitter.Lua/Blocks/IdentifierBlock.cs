using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System.Text;

namespace Bridge.Translator.Lua
{
    public class IdentifierBlock : ConversionBlock
    {
        public IdentifierBlock(IEmitter emitter, IdentifierExpression identifierExpression)
            : base(emitter, identifierExpression)
        {
            this.Emitter = emitter;
            this.IdentifierExpression = identifierExpression;
        }

        public IdentifierExpression IdentifierExpression
        {
            get;
            set;
        }

        protected override Expression GetExpression()
        {
            return this.IdentifierExpression;
        }

        protected override void EmitConversionExpression()
        {
            this.VisitIdentifierExpression();
        }

        protected void VisitIdentifierExpression()
        {
            IdentifierExpression identifierExpression = this.IdentifierExpression;
            int pos = this.Emitter.Output.Length;
            ResolveResult resolveResult = null;

            resolveResult = this.Emitter.Resolver.ResolveNode(identifierExpression, this.Emitter);

            var id = identifierExpression.Identifier;

            var isResolved = resolveResult != null && !(resolveResult is ErrorResolveResult);
            var memberResult = resolveResult as MemberResolveResult;

            if (this.Emitter.Locals != null && this.Emitter.Locals.ContainsKey(id))
            {
                if (this.Emitter.LocalsMap != null && this.Emitter.LocalsMap.ContainsKey(id) && !(identifierExpression.Parent is DirectionExpression))
                {
                    this.Write(this.Emitter.LocalsMap[id]);
                }
                else if (this.Emitter.LocalsNamesMap != null && this.Emitter.LocalsNamesMap.ContainsKey(id))
                {
                    this.Write(this.Emitter.LocalsNamesMap[id]);
                }
                else
                {
                    this.Write(id);
                }

                Helpers.CheckValueTypeClone(resolveResult, identifierExpression, this, pos);

                return;
            }

            if (resolveResult is TypeResolveResult)
            {
                this.Write(BridgeTypes.ToJsName(resolveResult.Type, this.Emitter));
                /*
                if (this.Emitter.Validator.IsIgnoreType(resolveResult.Type.GetDefinition()))
                {
                    this.Write(BridgeTypes.ToJsName(resolveResult.Type, this.Emitter));
                }
                else
                {
                    this.Write("Bridge.get(" + BridgeTypes.ToJsName(resolveResult.Type, this.Emitter) + ")");
                }*/
                return;
            }

            string inlineCode = memberResult != null ? this.Emitter.GetInline(memberResult.Member) : null;
            bool hasInline = !string.IsNullOrEmpty(inlineCode);
            bool hasThis = hasInline && inlineCode.Contains("{this}");

            if (hasThis)
            {
                this.Write("");
                var oldBuilder = this.Emitter.Output;
                this.Emitter.Output = new StringBuilder();

                if (memberResult.Member.IsStatic)
                {
                    this.Write(BridgeTypes.ToJsName(memberResult.Member.DeclaringType, this.Emitter));
                    /*
                    if (!this.Emitter.Validator.IsIgnoreType(memberResult.Member.DeclaringTypeDefinition))
                    {
                        this.Write("(Bridge.get(" + BridgeTypes.ToJsName(memberResult.Member.DeclaringType, this.Emitter) + "))");
                    }
                    else
                    {
                        this.Write(BridgeTypes.ToJsName(memberResult.Member.DeclaringType, this.Emitter));
                    }*/
                }
                else
                {
                    this.WriteThis();
                }

                inlineCode = inlineCode.Replace("{this}", this.Emitter.Output.ToString());
                this.Emitter.Output = oldBuilder;

                if (resolveResult is InvocationResolveResult)
                {
                    this.PushWriter(inlineCode);
                }
                else
                {
                    this.Write(inlineCode);
                }

                return;
            }

            if (hasInline && memberResult.Member.IsStatic)
            {
                if (resolveResult is InvocationResolveResult)
                {
                    this.PushWriter(inlineCode);
                }
                else
                {
                    this.Write(inlineCode);
                }
            }

            string appendAdditionalCode = null;
            if (memberResult != null &&
                        memberResult.Member is IMethod &&
                        !(memberResult is InvocationResolveResult) &&
                        !(
                        identifierExpression.Parent is InvocationExpression &&
                        identifierExpression.NextSibling != null &&
                        identifierExpression.NextSibling.Role is TokenRole &&
                        ((TokenRole)identifierExpression.NextSibling.Role).Token == "("
                        )
                )
            {
                var resolvedMethod = (IMethod)memberResult.Member;
                bool isStatic = resolvedMethod != null && resolvedMethod.IsStatic;

                if (!isStatic)
                {
                    var isExtensionMethod = resolvedMethod.IsExtensionMethod;
                    this.Write(Bridge.Translator.Emitter.ROOT + "." + (isExtensionMethod ? Bridge.Translator.Emitter.DELEGATE_BIND_SCOPE : Bridge.Translator.Emitter.DELEGATE_BIND) + "(");
                    this.WriteThis();
                    this.Write(", ");
                    appendAdditionalCode = ")";
                }
            }
            
            
            if (memberResult != null && memberResult.Member.SymbolKind == SymbolKind.Property && memberResult.TargetResult.Type.Kind != TypeKind.Anonymous)
            {
                bool isStatement = false;
                string valueVar = null;

                if (this.Emitter.IsUnaryAccessor)
                {
                    isStatement = identifierExpression.Parent is UnaryOperatorExpression && identifierExpression.Parent.Parent is ExpressionStatement;

                    if (NullableType.IsNullable(memberResult.Type))
                    {
                        isStatement = false;
                    }

                    if (!isStatement)
                    {
                        this.WriteOpenParentheses();

                        valueVar = this.GetTempVarName();

                        this.Write(valueVar);
                        this.Write(" = ");
                    }
                }

                this.WriteTarget(memberResult);

                if (!string.IsNullOrWhiteSpace(inlineCode))
                {
                    this.Write(inlineCode);
                }
                else if (Helpers.IsFieldProperty(memberResult.Member, this.Emitter))
                {
                    this.Write(Helpers.GetPropertyRef(memberResult.Member, this.Emitter));
                }
                else if (!this.Emitter.IsAssignment)
                {
                    if (this.Emitter.IsUnaryAccessor)
                    {
                        bool isDecimal = Helpers.IsDecimalType(memberResult.Member.ReturnType, this.Emitter.Resolver);
                        bool isNullable = NullableType.IsNullable(memberResult.Member.ReturnType);
                        if (isStatement)
                        {
                            this.Write(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, true));
                            this.WriteOpenParentheses();

                            if (isDecimal)
                            {
                                if (isNullable)
                                {
                                    this.Write("Bridge.Nullable.lift1");
                                    this.WriteOpenParentheses();
                                    if (this.Emitter.UnaryOperatorType == UnaryOperatorType.Increment ||
                                        this.Emitter.UnaryOperatorType == UnaryOperatorType.PostIncrement)
                                    {
                                        this.WriteScript("inc");
                                    }
                                    else
                                    {
                                        this.WriteScript("dec");
                                    }

                                    this.WriteComma();

                                    this.WriteTarget(memberResult);

                                    this.Write(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, false));
                                    this.WriteOpenParentheses();
                                    this.WriteCloseParentheses();
                                    this.WriteCloseParentheses();
                                }
                                else
                                {
                                    this.WriteTarget(memberResult);
                                    this.Write(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, false));
                                    this.WriteOpenParentheses();
                                    this.WriteCloseParentheses();
                                    this.WriteDot();

                                    if (this.Emitter.UnaryOperatorType == UnaryOperatorType.Increment ||
                                        this.Emitter.UnaryOperatorType == UnaryOperatorType.PostIncrement)
                                    {
                                        this.Write("inc");
                                    }
                                    else
                                    {
                                        this.Write("dec");
                                    }

                                    this.WriteOpenParentheses();
                                    this.WriteCloseParentheses();
                                }
                            }
                            else
                            {
                                this.WriteTarget(memberResult);

                                this.Write(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, false));
                                this.WriteOpenParentheses();
                                this.WriteCloseParentheses();

                                if (this.Emitter.UnaryOperatorType == UnaryOperatorType.Increment || this.Emitter.UnaryOperatorType == UnaryOperatorType.PostIncrement)
                                {
                                    this.Write("+");
                                }
                                else
                                {
                                    this.Write("-");
                                }

                                this.Write("1");
                            }

                            this.WriteCloseParentheses();
                        }
                        else
                        {
                            this.Write(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, false));
                            this.WriteOpenParentheses();
                            this.WriteCloseParentheses();
                            this.WriteComma();

                            this.WriteTarget(memberResult);
                            this.Write(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, true));
                            this.WriteOpenParentheses();

                            if (isDecimal)
                            {
                                if (isNullable)
                                {
                                    this.Write("Bridge.Nullable.lift1");
                                    this.WriteOpenParentheses();
                                    if (this.Emitter.UnaryOperatorType == UnaryOperatorType.Increment ||
                                        this.Emitter.UnaryOperatorType == UnaryOperatorType.PostIncrement)
                                    {
                                        this.WriteScript("inc");
                                    }
                                    else
                                    {
                                        this.WriteScript("dec");
                                    }

                                    this.WriteComma();
                                    this.Write(valueVar);
                                    this.WriteCloseParentheses();
                                }
                                else
                                {
                                    this.Write(valueVar);

                                    this.WriteDot();

                                    if (this.Emitter.UnaryOperatorType == UnaryOperatorType.Increment ||
                                        this.Emitter.UnaryOperatorType == UnaryOperatorType.PostIncrement)
                                    {
                                        this.Write("inc");
                                    }
                                    else
                                    {
                                        this.Write("dec");
                                    }

                                    this.WriteOpenParentheses();
                                    this.WriteCloseParentheses();
                                }
                            }
                            else
                            {
                                this.Write(valueVar);

                                if (this.Emitter.UnaryOperatorType == UnaryOperatorType.Increment || this.Emitter.UnaryOperatorType == UnaryOperatorType.PostIncrement)
                                {
                                    this.Write("+");
                                }
                                else
                                {
                                    this.Write("-");
                                }

                                this.Write("1");
                            }

                            this.WriteCloseParentheses();
                            this.WriteComma();

                            if (this.Emitter.UnaryOperatorType == UnaryOperatorType.Increment ||
                                this.Emitter.UnaryOperatorType == UnaryOperatorType.Decrement)
                            {
                                this.WriteTarget(memberResult);
                                this.Write(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, false));
                                this.WriteOpenParentheses();
                                this.WriteCloseParentheses();
                            }
                            else
                            {
                                this.Write(valueVar);
                            }

                            this.WriteCloseParentheses();

                            if (valueVar != null)
                            {
                                this.RemoveTempVar(valueVar);
                            }
                        }
                    }
                    else
                    {
                        this.Write(Helpers.GetPropertyRef(memberResult.Member, this.Emitter));
                        this.WriteOpenParentheses();
                        if(!memberResult.Member.IsStatic && memberResult.Member.IsInternalMember()) {
                            this.WriteThis();
                        }
                        this.WriteCloseParentheses();
                    }
                }
                else if (this.Emitter.AssignmentType != AssignmentOperatorType.Assign)
                {
                    this.PushWriter(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, true) + "({0})");
                    /*
                    string trg;

                    if (memberResult.Member.IsStatic)
                    {
                        trg = BridgeTypes.ToJsName(memberResult.Member.DeclaringType, this.Emitter);
                    }
                    else
                    {
                        trg = "this";
                    }

                    this.PushWriter(string.Concat(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, true),
                        "(",
                        trg,
                        ".",
                        Helpers.GetPropertyRef(memberResult.Member, this.Emitter, false),
                        "()",
                        "{0})"));*/
                }
                else
                {
                    string argsFormatStr;
                    if(!memberResult.Member.IsStatic && memberResult.Member.DeclaringType == TransformCtx.CurClass) {
                        argsFormatStr = "(this, {0})";
                    }
                    else {
                        argsFormatStr = "({0})";
                    }

                    this.PushWriter(Helpers.GetPropertyRef(memberResult.Member, this.Emitter, true) + argsFormatStr);
                }
            }
            else if (memberResult != null && memberResult.Member is DefaultResolvedEvent)
            {
                if (this.Emitter.IsAssignment &&
                    (this.Emitter.AssignmentType == AssignmentOperatorType.Add ||
                     this.Emitter.AssignmentType == AssignmentOperatorType.Subtract))
                {
                    this.WriteTarget(memberResult);

                    if (!string.IsNullOrWhiteSpace(inlineCode))
                    {
                        this.Write(inlineCode);
                    }
                    else
                    {
                        this.Write(this.Emitter.AssignmentType == AssignmentOperatorType.Add ? "add" : "remove");
                        this.Write(
                            OverloadsCollection.Create(this.Emitter, memberResult.Member,
                                this.Emitter.AssignmentType == AssignmentOperatorType.Subtract).GetOverloadName());
                    }

                    this.WriteOpenParentheses();
                }
                else
                {
                    this.WriteTarget(memberResult);
                    this.Write(this.Emitter.GetEntityName(memberResult.Member, true));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(inlineCode))
                {
                    this.Write(inlineCode);
                }
                else if (isResolved)
                {
                    if (resolveResult is TypeResolveResult)
                    {
                        var typeResolveResult = (TypeResolveResult)resolveResult;
                        this.Write(BridgeTypes.ToJsName(typeResolveResult.Type, this.Emitter));

                        /*
                        var isNative = this.Emitter.Validator.IsIgnoreType(typeResolveResult.Type.GetDefinition());
                        if (!isNative)
                        {
                            this.Write("Bridge.get(" + BridgeTypes.ToJsName(typeResolveResult.Type, this.Emitter));
                        }
                        else
                        {
                            this.Write(BridgeTypes.ToJsName(typeResolveResult.Type, this.Emitter));
                        }*/
                        
                        if (typeResolveResult.Type.TypeParameterCount > 0)
                        {
                            this.WriteOpenParentheses();
                            new TypeExpressionListBlock(this.Emitter, this.IdentifierExpression.TypeArguments).Emit();
                            this.WriteCloseParentheses();
                        }

                        /*
                        if (!isNative)
                        {
                            this.Write(")");
                        }*/                        
                    }
                    else if (resolveResult is LocalResolveResult)
                    {
                        if(resolveResult.IsCompileTimeConstant) {
                            this.Write(resolveResult.ConstantValue);
                        }
                        else {
                            var localResolveResult = (LocalResolveResult)resolveResult;
                            this.Write(localResolveResult.Variable.Name);
                        }
                    }
                    else if (memberResult != null)
                    {
                        if(this.Emitter.IsMemberConst(memberResult.Member)) {
                            this.Write(memberResult.ConstantValue);
                        }
                        else {
                            this.WriteTarget(memberResult);
                            this.Write(OverloadsCollection.Create(this.Emitter, memberResult.Member).GetOverloadName());
                        }
                    }
                    else
                    {
                        this.Write(resolveResult.ToString());
                    }
                }
                else
                {
                    throw new EmitterException(identifierExpression, "Cannot resolve identifier: " + id);
                }
            }

            if (appendAdditionalCode != null)
            {
                this.Write(appendAdditionalCode);
            }

            Helpers.CheckValueTypeClone(resolveResult, identifierExpression, this, pos);
        }

        protected void WriteTarget(MemberResolveResult memberResult)
        {            
            if (memberResult.Member.IsStatic)
            {
                switch(memberResult.Member.SymbolKind) {
                    case SymbolKind.Method: {
                            if(memberResult.Member.DeclaringType != TransformCtx.CurClass) {
                                this.Write(BridgeTypes.ToJsName(memberResult.Member.DeclaringType, this.Emitter));
                                this.WriteDot();
                            }
                            break;
                        }
                    case SymbolKind.Property: {
                            if(Helpers.IsFieldProperty(memberResult.Member, this.Emitter)) {
                                this.Write(BridgeTypes.ToJsName(memberResult.Member.DeclaringType, this.Emitter));
                                this.WriteDot();
                            }
                            else {
                                if(memberResult.Member.DeclaringType != TransformCtx.CurClass) {
                                    this.Write(BridgeTypes.ToJsName(memberResult.Member.DeclaringType, this.Emitter));
                                    this.WriteDot();
                                }
                            }
                            break;
                        }
                    default: {
                            this.Write(BridgeTypes.ToJsName(memberResult.Member.DeclaringType, this.Emitter));
                            this.WriteDot();
                            break;
                        }
                }
            }
            else
            {
                switch(memberResult.Member.SymbolKind) {
                    case SymbolKind.Method: {
                            if(!memberResult.Member.IsInternalMember()) {
                                this.WriteThis();
                                if(this.IdentifierExpression.Role == Roles.TargetExpression) {
                                    this.WriteObjectColon();
                                }
                                else {
                                    this.WriteDot();
                                }
                            }
                            break;
                        }
                    case SymbolKind.Property: {
                            if(Helpers.IsFieldProperty(memberResult.Member, this.Emitter)) {
                                this.WriteThis();
                                this.WriteDot();
                            }
                            else {
                                if(!memberResult.Member.IsInternalMember()) {
                                    this.WriteThis();
                                    this.WriteObjectColon();
                                }
                            }
                            break;
                        }
                    default: {
                            this.WriteThis();
                            this.WriteDot();
                            break;
                        }
                }
            }
        }
    }
}
