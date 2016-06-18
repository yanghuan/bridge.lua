using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using System.Collections.Generic;

namespace Bridge.Translator.TypeScript
{
    public class MethodBlock : AbstractEmitterBlock
    {
        public MethodBlock(IEmitter emitter, MethodDeclaration methodDeclaration)
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
            XmlToJsDoc.EmitComment(this, this.MethodDeclaration);
            var overloads = OverloadsCollection.Create(this.Emitter, methodDeclaration);

            if (overloads.HasOverloads)
            {
                string name = overloads.GetOverloadName();
                this.Write(name);
            }
            else
            {
                this.Write(this.Emitter.GetEntityName(methodDeclaration));
            }

            var isGeneric = methodDeclaration.TypeParameters.Count > 0;
            if (isGeneric)
            {
                bool needComma = false;
                this.Write("<");
                foreach (var p in methodDeclaration.TypeParameters)
                {
                    if (needComma)
                    {
                        this.WriteComma();
                    }

                    needComma = true;
                    this.Write(p.Name);
                }
                this.Write(">");

                this.WriteOpenParentheses();

                var comma = false;
                foreach (var p in methodDeclaration.TypeParameters)
                {
                    if (comma)
                    {
                        this.WriteComma();
                    }
                    this.Write(p.Name);
                    this.WriteColon();
                    this.WriteOpenBrace();
                    this.Write("prototype");
                    this.WriteColon();
                    this.Write(p.Name);

                    this.WriteCloseBrace();
                    comma = true;
                }

                this.WriteCloseParentheses();

                this.WriteColon();
                this.WriteOpenBrace();
                this.WriteSpace();
            }

            this.EmitMethodParameters(methodDeclaration.Parameters, methodDeclaration);
            this.WriteColon();

            var retType = BridgeTypes.ToTypeScriptName(methodDeclaration.ReturnType, this.Emitter);
            this.Write(retType);

            if (isGeneric)
            {
                this.WriteSpace();
                this.WriteCloseBrace();
            }

            this.WriteSemiColon();
            this.WriteNewLine();
        }

        protected virtual void EmitMethodParameters(IEnumerable<ParameterDeclaration> declarations, AstNode context)
        {
            this.WriteOpenParentheses();
            bool needComma = false;

            foreach (var p in declarations)
            {
                var name = this.Emitter.GetEntityName(p);
                bool optional = p.DefaultExpression != null && !p.DefaultExpression.IsNull;

                if (needComma)
                {
                    this.WriteComma();
                }

                needComma = true;
                this.Write(name);

                if (optional)
                {
                    this.Write("?");
                }

                this.WriteColon();
                name = BridgeTypes.ToTypeScriptName(p.Type, this.Emitter);
                if (p.ParameterModifier == ParameterModifier.Out || p.ParameterModifier == ParameterModifier.Ref)
                {
                    name = "{v: " + name + "}";
                }
                this.Write(name);
            }

            this.WriteCloseParentheses();
        }
    }
}
