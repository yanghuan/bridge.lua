using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Mono.Cecil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Translator.Lua
{
    public partial class Emitter
    {
        protected virtual HashSet<string> CreateNamespaces()
        {
            var result = new HashSet<string>();

            foreach (string typeName in this.TypeDefinitions.Keys)
            {
                int index = typeName.LastIndexOf('.');

                if (index >= 0)
                {
                    this.RegisterNamespace(typeName.Substring(0, index), result);
                }
            }

            return result;
        }

        protected virtual void RegisterNamespace(string ns, ICollection<string> repository)
        {
            if (String.IsNullOrEmpty(ns) || repository.Contains(ns))
            {
                return;
            }

            string[] parts = ns.Split('.');
            StringBuilder builder = new StringBuilder();

            foreach (string part in parts)
            {
                if (builder.Length > 0)
                {
                    builder.Append('.');
                }

                builder.Append(part);
                string item = builder.ToString();

                if (!repository.Contains(item))
                {
                    repository.Add(item);
                }
            }
        }

        public static bool IsReservedStaticName(string name)
        {
            return Emitter.reservedStaticNames.Any(n => String.Equals(name, n, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual string ToJavaScript(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        protected virtual ICSharpCode.NRefactory.CSharp.Attribute GetAttribute(AstNodeCollection<AttributeSection> attributes, string name)
        {
            string fullName = name + "Attribute";
            foreach (var i in attributes)
            {
                foreach (var j in i.Attributes)
                {
                    if (j.Type.ToString() == name)
                    {
                        return j;
                    }

                    var resolveResult = this.Resolver.ResolveNode(j, this);
                    if (resolveResult != null && resolveResult.Type != null && resolveResult.Type.FullName == fullName)
                    {
                        return j;
                    }
                }
            }

            return null;
        }

        public virtual CustomAttribute GetAttribute(IEnumerable<CustomAttribute> attributes, string name)
        {
            foreach (var attr in attributes)
            {
                if (attr.AttributeType.FullName == name)
                {
                    return attr;
                }
            }

            return null;
        }

        public virtual IAttribute GetAttribute(IEnumerable<IAttribute> attributes, string name)
        {
            foreach (var attr in attributes)
            {
                if (attr.AttributeType.FullName == name)
                {
                    return attr;
                }
            }

            return null;
        }

        protected virtual bool HasDelegateAttribute(MethodDeclaration method)
        {
            return this.GetAttribute(method.Attributes, "Delegate") != null;
        }

        public virtual Tuple<bool, bool, string> GetInlineCode(MemberReferenceExpression node)
        {
            var member = LiftNullableMember(node);
            return GetInlineCodeFromMember(member, node);
        }
        
        public virtual Tuple<bool, bool, string> GetInlineCode(InvocationExpression node)
        {
            var target = node.Target as MemberReferenceExpression;
            IMember member = null;
            if (target != null)
            {
                member = LiftNullableMember(target);
            }

            return GetInlineCodeFromMember(member, node);
        }

        private Tuple<bool, bool, string> GetInlineCodeFromMember(IMember member, Expression node) {
            if(member == null) {
                var resolveResult = this.Resolver.ResolveNode(node, this);
                var memberResolveResult = resolveResult as MemberResolveResult;

                if(memberResolveResult == null) {
                    return new Tuple<bool, bool, string>(false, false, null);
                }

                member = memberResolveResult.Member;
            }

            bool isInlineMethod = this.IsInlineMethod(member);
            var inlineCode = isInlineMethod ? null : this.GetInline(member);
            var isStatic = member.IsStatic;
            return new Tuple<bool, bool, string>(isStatic, isInlineMethod, inlineCode);
        }

        private IMember LiftNullableMember(MemberReferenceExpression target)
        {
            var targetrr = this.Resolver.ResolveNode(target.Target, this);
            IMember member = null;
            if (targetrr.Type.IsKnownType(KnownTypeCode.NullableOfT))
            {
                string name = null;
                int count = 0;
                if (target.MemberName == "ToString" || target.MemberName == "GetHashCode")
                {
                    name = target.MemberName;
                }
                else if (target.MemberName == "Equals")
                {
                    name = target.MemberName;
                    count = 1;
                }

                if (name != null)
                {
                    var type = ((ParameterizedType) targetrr.Type).TypeArguments[0];
                    member = type.GetMethods(null, GetMemberOptions.IgnoreInheritedMembers)
                            .FirstOrDefault(m => m.Name == name && m.Parameters.Count == count);
                }
            }
            return member;
        }

        public virtual bool IsForbiddenInvocation(InvocationExpression node)
        {
            var resolveResult = this.Resolver.ResolveNode(node, this);
            var memberResolveResult = resolveResult as MemberResolveResult;

            if (memberResolveResult == null)
            {
                return false;
            }

            var member = memberResolveResult.Member;

            string attrName = Bridge.Translator.Translator.Bridge_ASSEMBLY + ".InitAttribute";

            if (member != null)
            {
                var attr = member.Attributes.FirstOrDefault(a =>
                {
                    return a.AttributeType.FullName == attrName;
                });

                if (attr != null)
                {
                    if (attr.PositionalArguments.Count > 0)
                    {
                        var argExpr = attr.PositionalArguments.First();
                        if (argExpr.ConstantValue is int)
                        {
                            var value = (int)argExpr.ConstantValue;

                            if (value > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public virtual IEnumerable<string> GetScript(EntityDeclaration method)
        {
            var attr = this.GetAttribute(method.Attributes, Bridge.Translator.Translator.Bridge_ASSEMBLY + ".Script");
            return this.GetScriptArguments(attr);
        }

        private string GetMetaName(IEntity member) {
            string name = null;
            if(TransformCtx.GetEntityName != null) {
                name = TransformCtx.GetEntityName(member);
            }
            else {
                switch(member.SymbolKind) {
                    case SymbolKind.Property: {
                            name = XmlMetaMaker.GetPropertyName((IProperty)member);
                            break;
                        }
                    case SymbolKind.Method: {
                            name = XmlMetaMaker.GetMethodName((IMethod)member);
                            break;
                        }
                }
            }

            if(name != null) {
                if(Helpers.IsReservedWord(name)) {
                    throw new System.Exception("GetMetaName[{0}, {1}, {2}] IsReservedWord".F(name, member.Name, member.DeclaringType.FullName));
                }
            }

            return name;
        }

        public string GetEntityName(IEntity member, bool forcePreserveMemberCase, out bool isMetaName) {
            isMetaName = false;
            string name = GetMetaName(member);
            if(name != null) {
                isMetaName = true;
                return name;
            }
           
            if(member.SymbolKind == SymbolKind.Constructor) {
                name = "constructor";
            }
            else {
                name = member.Name;
            }

            Helpers.FixReservedWord(ref name);
            return name;
        }

        public string GetEntityName(IEntity member, bool forcePreserveMemberCase = false, bool ignoreInterface = false)
        {
            bool _;
            return GetEntityName(member, forcePreserveMemberCase, out _);
        }

        public virtual string GetEntityName(EntityDeclaration entity, bool forcePreserveMemberCase = false, bool ignoreInterface = false)
        {
            var rr = this.Resolver.ResolveNode(entity, this) as MemberResolveResult;
            if (rr != null)
            {
                return this.GetEntityName(rr.Member, forcePreserveMemberCase, ignoreInterface);
            }
            return null;
        }

        public virtual string GetEntityName(ParameterDeclaration entity, bool forcePreserveMemberCase = false)
        {
            var name = entity.Name;

            if (entity.Parent != null && entity.GetParent<SyntaxTree>() != null)
            {
                var rr = this.Resolver.ResolveNode(entity, this) as LocalResolveResult;
                if (rr != null)
                {
                    var iparam = rr.Variable as IParameter;

                    if (iparam != null && iparam.Attributes != null)
                    {
                        var attr = iparam.Attributes.FirstOrDefault(a => a.AttributeType.FullName == Bridge.Translator.Translator.Bridge_ASSEMBLY + ".NameAttribute");

                        if (attr != null)
                        {
                            var value = attr.PositionalArguments.First().ConstantValue;
                            if (value is string)
                            {
                                name = value.ToString();
                                FixBridgePrefix(ref name);
                            }
                        }
                    }
                }
            }

            if (Helpers.IsReservedWord(name))
            {
                name = Helpers.ChangeReservedWord(name);
            }

            return name;
        }

        public virtual string GetFieldName(FieldDeclaration field)
        {
            if (!string.IsNullOrEmpty(field.Name))
            {
                return field.Name;
            }

            if (field.Variables.Count > 0)
            {
                return field.Variables.First().Name;
            }

            return null;
        }

        public virtual string GetEventName(EventDeclaration evt)
        {
            if (!string.IsNullOrEmpty(evt.Name))
            {
                return evt.Name;
            }

            if (evt.Variables.Count > 0)
            {
                return evt.Variables.First().Name;
            }

            return null;
        }

        public Tuple<bool, string> IsGlobalTarget(IMember member)
        {
            var attr = this.GetAttribute(member.Attributes, Bridge.Translator.Translator.Bridge_ASSEMBLY + ".GlobalTargetAttribute");

            return attr != null ? new Tuple<bool, string>(true, (string)attr.PositionalArguments.First().ConstantValue) : null;
        }

        public virtual string GetInline(ICustomAttributeProvider provider)
        {
            var attr = this.GetAttribute(provider.CustomAttributes, Bridge.Translator.Translator.Bridge_ASSEMBLY + ".TemplateAttribute");

            return attr != null && attr.ConstructorArguments.Count > 0 ? ((string)attr.ConstructorArguments.First().Value) : null;
        }

        public virtual string GetInline(EntityDeclaration method)
        {
            var attr = this.GetAttribute(method.Attributes, Bridge.Translator.Translator.Bridge_ASSEMBLY + ".Template");

            return attr != null && attr.Arguments.Count > 0 ? ((string)((PrimitiveExpression)attr.Arguments.First()).Value) : null;
        }

        public static void FixBridgePrefix(ref string s) {
            if(!string.IsNullOrEmpty(s)) {
                string bridgePrefix = Bridge.Translator.Translator.Bridge_ASSEMBLY + '.';
                if(s.StartsWith(bridgePrefix)) {
                    s = "System." + s.Substring(bridgePrefix.Length);
                }
            }
        }

        public virtual string GetInline(IEntity entity)
        {
            switch(entity.SymbolKind) {
                case SymbolKind.Property: {
                        IProperty property = (IProperty)entity;
                        bool isGetOrSet = !IsAssignment;
                        return XmlMetaMaker.GetPropertyInline(property, isGetOrSet);
                    }
                case SymbolKind.Method: {
                        IMethod method = (IMethod)entity;
                        return XmlMetaMaker.GetMethodInline(method);
                    }
            }

            string attrName = Bridge.Translator.Translator.Bridge_ASSEMBLY + ".TemplateAttribute";
            if (entity != null)
            {
                var attr = entity.Attributes.FirstOrDefault(a =>
                {
                    return a.AttributeType.FullName == attrName;
                });

                string code = attr != null && attr.PositionalArguments.Count > 0 ? attr.PositionalArguments[0].ConstantValue.ToString() : null;
                FixBridgePrefix(ref code);
                return code;
            }

            return null;
        }

        private bool IsInlineMethod(IEntity entity) {
            return false;
        }

        protected virtual IEnumerable<string> GetScriptArguments(ICSharpCode.NRefactory.CSharp.Attribute attr)
        {
            if (attr == null)
            {
                return null;
            }

            var result = new List<string>();

            foreach (var arg in attr.Arguments)
            {
                PrimitiveExpression expr = (PrimitiveExpression)arg;
                result.Add((string)expr.Value);
            }

            return result;
        }

        public virtual bool IsNativeMember(string fullName)
        {
            return fullName.StartsWith(Bridge.Translator.Translator.Bridge_ASSEMBLY + ".") || fullName.StartsWith("System.");
        }

        public virtual bool IsMemberConst(IMember member)
        {
            DefaultResolvedField defaultResolvedField = member as DefaultResolvedField;
            if(defaultResolvedField != null) {
                return defaultResolvedField.IsConst;
            }

            SpecializedField specializedField = member as SpecializedField;
            if(specializedField != null) {
                return specializedField.IsConst;
            }

            return false;
        }

        public virtual bool IsInlineConst(IMember member)
        {
            bool isConst = IsMemberConst(member);
            if (isConst)
            {
                return true;
                /*
                var attr = this.GetAttribute(member.Attributes, Bridge.Translator.Translator.Bridge_ASSEMBLY + ".InlineConstAttribute");
                if (attr != null)
                {
                    return true;
                }*/
            }
            return false;
        }

        public virtual void InitEmitter()
        {
            this.Output = new StringBuilder();
            this.Locals = null;
            this.LocalsStack = null;
            this.IteratorCount = 0;
            this.ThisRefCounter = 0;
            this.Writers = new Stack<Tuple<string, StringBuilder, bool, Action>>();
            this.IsAssignment = false;
            this.Level = 0;
            this.IsNewLine = true;
            this.EnableSemicolon = true;
            this.Comma = false;
            this.CurrentDependencies = new List<IPluginDependency>();
        }
    }
}
