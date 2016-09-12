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
        public static bool IsReservedStaticName(string name)
        {
            return Emitter.reservedStaticNames.Any(n => String.Equals(name, n, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual string ToJavaScript(object value)
        {
            return JsonConvert.SerializeObject(value);
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
            var inlineCode = this.GetInline(member);
            var isStatic = member.IsStatic;
            return new Tuple<bool, bool, string>(isStatic, false, inlineCode);
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

        public virtual IEnumerable<string> GetScript(EntityDeclaration method)
        {
            throw new System.NotSupportedException();
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
            throw new NotSupportedException();
        }

        public virtual string GetInline(ICustomAttributeProvider provider)
        {
            throw new NotSupportedException();
        }

        public virtual string GetInline(EntityDeclaration method)
        {
            throw new NotSupportedException();
        }

        public virtual string GetInline(IEntity entity)
        {
            switch(entity.SymbolKind) {
                case SymbolKind.Field: {
                        IField field = (IField)entity;
                        return XmlMetaMaker.GetFieldInline(field);
                    }
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
            return null;
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
