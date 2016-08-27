using System;
using System.Linq;
using System.Collections.Generic;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

using Bridge.Contract;

namespace Bridge.Translator.Lua
{
    public class FieldBlock : AbstractEmitterBlock
    {
        public FieldBlock(IEmitter emitter, ITypeInfo typeInfo, bool staticBlock, bool fieldsOnly)
            : base(emitter, typeInfo.TypeDeclaration)
        {
            this.Emitter = emitter;
            this.TypeInfo = typeInfo;
            this.StaticBlock = staticBlock;
            this.FieldsOnly = fieldsOnly;
            this.Injectors = new List<string>();
        }

        public ITypeInfo TypeInfo
        {
            get;
            set;
        }

        public bool StaticBlock
        {
            get;
            set;
        }

        public bool FieldsOnly
        {
            get;
            set;
        }

        public List<string> Injectors
        {
            get;
            private set;
        }

        public bool WasEmitted
        {
            get;
            private set;
        }

        protected override void DoEmit()
        {
            this.EmitFields(this.StaticBlock ? this.TypeInfo.StaticConfig : this.TypeInfo.InstanceConfig);
        }

        protected enum FieldTypeEnum {
            Field,
            Property,
            Event,
        }

        protected virtual void EmitFields(TypeConfigInfo info)
        {
            if (this.FieldsOnly)
            {
                if (info.Fields.Count > 0)
                {
                    this.WriteObject(null, info.Fields.Where(i => !i.IsConst).ToList(), (m, v) => string.Format("this.{0} = {1}", m.GetName(this.Emitter), v), FieldTypeEnum.Field);
                }
                return;
            }

            if (info.Events.Count > 0)
            {
                this.Indent();
                this.WriteObject(null, info.Events, (m, v) => {
                    string name = m.GetName(this.Emitter);
                    bool isPrivate = m.Entity.HasModifier(Modifiers.Private);
                    TransformCtx.CurClassOtherMethodNames.Add(new TransformCtx.MethodInfo() { Name = "add" + name, IsPrivate = isPrivate });
                    TransformCtx.CurClassOtherMethodNames.Add(new TransformCtx.MethodInfo() { Name = "remove" + name, IsPrivate = isPrivate });
                    return string.Format("add{0}, remove{0} = System.event(this, \"{0}\", {1})", name, v);
                }, FieldTypeEnum.Event);
                this.Outdent();
            }

            if (info.Properties.Count > 0)
            {
                this.WriteObject(null, info.Properties, (m, v) => {
                    string name = m.GetName(this.Emitter);
                    bool isPrivate = m.Entity.HasModifier(Modifiers.Private);
                    TransformCtx.CurClassOtherMethodNames.Add(new TransformCtx.MethodInfo() { Name = "get" + name, IsPrivate = isPrivate });
                    TransformCtx.CurClassOtherMethodNames.Add(new TransformCtx.MethodInfo() { Name = "set" + name, IsPrivate = isPrivate });
                    return string.Format("get{0}, set{0} = System.property(this, \"{0}\", {1})", name, v);
                }, FieldTypeEnum.Property);
            }

            if (info.Alias.Count > 0)
            {
                throw new System.NotSupportedException("Alias is not exists");
            }
        }

        protected virtual bool WriteObject(string objectName, List<TypeConfigItem> members, Func<TypeConfigItem, string, string> format, FieldTypeEnum type)
        {
            bool hasProperties = this.HasProperties(members);

            if (hasProperties && objectName != null)
            {
                this.EnsureComma();
                this.Write(objectName);

                this.WriteEqualsSign();
                this.BeginBlock();
            }

            foreach (var member in members)
            {
                object constValue = null;
                bool isPrimitive = false;
                var primitiveExpr = member.Initializer as PrimitiveExpression;
                if (primitiveExpr != null)
                {
                    isPrimitive = true;
                    constValue = primitiveExpr.Value;
                }

                var isNull = member.Initializer.IsNull || member.Initializer is NullReferenceExpression;

                if (!isNull && !isPrimitive)
                {
                    var resolveResult = this.Emitter.Resolver.ResolveNode(member.Initializer, this.Emitter);
                    if (resolveResult != null && resolveResult.IsCompileTimeConstant)
                    {
                        isPrimitive = true;
                        constValue = resolveResult.ConstantValue;
                    }
                }

                var isNullable = false;

                if (isPrimitive && constValue is AstType)
                {
                    var itype = this.Emitter.Resolver.ResolveNode((AstType)constValue, this.Emitter);

                    if (NullableType.IsNullable(itype.Type))
                    {
                        isNullable = true;
                    }
                }

                if(type == FieldTypeEnum.Property || type == FieldTypeEnum.Event) {
                    this.PushWriter("{0}");
                    member.Initializer.AcceptVisitor(this.Emitter);
                    string value = this.PopWriter(true);
                    this.Injectors.Add(format(member, value));
                    continue;
                }

                if (!isNull && (!isPrimitive || (constValue is AstType)))
                {
                    string value = null;

                    if (!isPrimitive)
                    {
                        this.PushWriter("{0}");
                        member.Initializer.AcceptVisitor(this.Emitter);
                        value = this.PopWriter(true);

                        //var oldWriter = this.SaveWriter();
                        //this.NewWriter();
                        //member.Initializer.AcceptVisitor(this.Emitter);
                        //value = this.Emitter.Output.ToString();
                        //this.RestoreWriter(oldWriter);
                    }
                    else
                    {
                        if (isNullable)
                        {
                            value = LuaHelper.Nil;
                        }
                        else
                        {
                            AstType astType = (AstType)constValue;
                            var rr = Emitter.Resolver.ResolveNode(astType, Emitter);
                            var def = Inspector.GetDefaultFieldValue(rr.Type);
                            if(def == rr.Type) {
                                value = Inspector.GetStructDefaultValue(rr.Type, this.Emitter);
                            }
                            else {
                                value = def.ToString();
                            }
                        }
                    }

                    this.Injectors.Add(format(member, value));
                    continue;
                }

                PushWriter("{0}");
                if(constValue is AstType) {
                    if(isNullable) {
                        this.Write(LuaHelper.Nil);
                    }
                    else {
                        AstType astType = (AstType)constValue;
                        var rr = Emitter.Resolver.ResolveNode(astType, Emitter);
                        var def = Inspector.GetDefaultFieldValue(rr.Type);
                        if(def == rr.Type) {
                            this.Write(Inspector.GetStructDefaultValue(rr.Type, this.Emitter));
                        }
                        else {
                            this.WriteScript(def);
                        }
                    }
                }
                else {
                    member.Initializer.AcceptVisitor(this.Emitter);
                }

                string fieldValue = PopWriter(true);
                if(fieldValue != LuaHelper.Nil) {
                    this.EnsureComma();
                    XmlToJsDoc.EmitComment(this, member.Entity);
                    this.Write(member.GetName(this.Emitter));
                    this.WriteEqualsSign();
                    this.Write(fieldValue);
                    this.Emitter.Comma = true;
                }
            }

            if (hasProperties && objectName != null)
            {
                this.WriteNewLine();
                this.EndBlock();
            }

            return hasProperties;
        }

        protected virtual bool HasProperties(List<TypeConfigItem> members)
        {
            foreach (var member in members)
            {
                object constValue = null;
                bool isPrimitive = false;
                var primitiveExpr = member.Initializer as PrimitiveExpression;
                if (primitiveExpr != null)
                {
                    isPrimitive = true;
                    constValue = primitiveExpr.Value;
                }

                var isNull = member.Initializer.IsNull || member.Initializer is NullReferenceExpression;

                if (!isNull && !isPrimitive)
                {
                    var constrr = this.Emitter.Resolver.ResolveNode(member.Initializer, this.Emitter) as ConstantResolveResult;
                    if (constrr != null)
                    {
                        isPrimitive = true;
                        constValue = constrr.ConstantValue;
                    }
                }

                if (isNull)
                {
                    return true;
                }

                if (!isPrimitive || (constValue is AstType))
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}
