using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using System.Linq;

namespace Bridge.Translator.Lua
{
    public class VisitorMethodBlock : AbstractMethodBlock
    {
        public VisitorMethodBlock(IEmitter emitter, MethodDeclaration methodDeclaration)
            : base(emitter, methodDeclaration)
        {
            this.Emitter = emitter;
            this.MethodDeclaration = methodDeclaration;
        }

        public MethodDeclaration MethodDeclaration
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            this.VisitMethodDeclaration(this.MethodDeclaration);
        }

        protected void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        {
            foreach (var attrSection in methodDeclaration.Attributes)
            {
                foreach (var attr in attrSection.Attributes)
                {
                    var rr = this.Emitter.Resolver.ResolveNode(attr.Type, this.Emitter);
                    if (rr.Type.FullName == "Bridge.ExternalAttribute" || rr.Type.FullName == "Bridge.IgnoreAttribute")
                    {
                        return;
                    }
                    else if (rr.Type.FullName == "Bridge.InitAttribute")
                    {
                        int initPosition = 0;

                        if (attr.HasArgumentList)
                        {
                            if (attr.Arguments.Any())
                            {
                                var argExpr = attr.Arguments.First();
                                var argrr = this.Emitter.Resolver.ResolveNode(argExpr, this.Emitter);
                                if (argrr.ConstantValue is int)
                                {
                                    initPosition = (int)argrr.ConstantValue;
                                }
                            }
                        }

                        if (initPosition > 0)
                        {
                            return;
                        }
                    }
                }
            }

            //this.EnsureComma();
            this.EnsureNewLine();
            this.ResetLocals();

            var prevMap = this.BuildLocalsMap();
            var prevNamesMap = this.BuildLocalsNamesMap();

            this.AddLocals(methodDeclaration.Parameters, methodDeclaration.Body);

            var typeDef = this.Emitter.GetTypeDefinition();
            var overloads = OverloadsCollection.Create(this.Emitter, methodDeclaration);
            XmlToJsDoc.EmitComment(this, this.MethodDeclaration);

            string name = overloads.GetOverloadName();
            TransformCtx.CurClassMethodNames.Add(new TransformCtx.MethodInfo() {
                Name = name,
                IsPrivate = methodDeclaration.HasModifier(Modifiers.Private),
            });

            this.Write(name);
            this.WriteEqualsSign();

            /*
            if (methodDeclaration.TypeParameters.Count > 0)
            {
                this.WriteFunction();
                this.EmitTypeParameters(methodDeclaration.TypeParameters, methodDeclaration);
                this.WriteSpace();
                this.BeginBlock();
                this.WriteReturn(true);
                this.Write("Bridge.fn.bind(this, ");
            }*/

            this.WriteFunction();

            this.EmitMethodParameters(methodDeclaration.Parameters, methodDeclaration, true);
            if(methodDeclaration.TypeParameters.Count > 0) {
                if(methodDeclaration.Parameters.Count > 0 || !methodDeclaration.HasModifier(Modifiers.Static)) {
                    this.WriteComma();
                }
                this.EmitTypeParameters(methodDeclaration.TypeParameters, methodDeclaration);
            }
            this.WriteCloseParentheses();

            this.WriteSpace();

            var script = this.Emitter.GetScript(methodDeclaration);

            if (script == null)
            {
                if (methodDeclaration.HasModifier(Modifiers.Async))
                {
                    new AsyncBlock(this.Emitter, methodDeclaration).Emit();
                }
                else
                {
                    this.BeginFunctionBlock();
                    methodDeclaration.Body.AcceptVisitor(this.Emitter);
                    PrimitiveType returnType = methodDeclaration.ReturnType as PrimitiveType;
                    if(returnType != null) {
                        if(returnType.KnownTypeCode == ICSharpCode.NRefactory.TypeSystem.KnownTypeCode.Void) {
                            string refArgsString = GetRefArgsString(this.Emitter, methodDeclaration);
                            if(refArgsString != null) {
                                this.WriteReturn(true);
                                this.Write(refArgsString);
                                this.WriteNewLine();
                            }
                        }
                    }
                    this.EndFunctionBlock();
                }
            }
            else
            {
                this.BeginBlock();

                foreach (var line in script)
                {
                    this.Write(line);
                    this.WriteNewLine();
                }

                this.EndBlock();
            }
            
            /*
            if (methodDeclaration.TypeParameters.Count > 0)
            {
                this.Write(");");
                this.WriteNewLine();
                this.EndBlock();
            }*/

            this.ClearLocalsMap(prevMap);
            this.ClearLocalsNamesMap(prevNamesMap);
            this.Emitter.Comma = true;
        }

        public static string GetRefArgsString(IEmitter Emitter, MethodDeclaration methodDeclaration) {
            var sb = new System.Text.StringBuilder();
            bool isFirst = true;
            foreach(var item in methodDeclaration.Parameters) {
                if(item.ParameterModifier == ParameterModifier.Out || item.ParameterModifier == ParameterModifier.Ref) {
                    if(isFirst) {
                        isFirst = false;
                    }
                    else {
                        sb.Append(", ");
                    }
                    string itemName = Emitter.LocalsMap[item.Name];
                    sb.Append(itemName);
                }
            }
            return sb.Length > 0 ?  sb.ToString(): null;
        }
    }
}
