using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using System.Collections.Generic;

namespace Bridge.Translator
{
    public abstract class AbstractMethodBlock : AbstractEmitterBlock
    {
        public AbstractMethodBlock(IEmitter emitter, AstNode node)
            : base(emitter, node)
        {
        }

        protected virtual void EmitMethodParameters(IEnumerable<ParameterDeclaration> declarations, AstNode context, bool skipCloseParentheses = false)
        {
            this.WriteOpenParentheses();
            bool needComma = false;

            foreach (var p in declarations)
            {
                var name = this.Emitter.GetEntityName(p);

                name = name.Replace(Bridge.Translator.Emitter.FIX_ARGUMENT_NAME, "");

                if (this.Emitter.LocalsNamesMap != null && this.Emitter.LocalsNamesMap.ContainsKey(name))
                {
                    name = this.Emitter.LocalsNamesMap[name];
                }

                if (needComma)
                {
                    this.WriteComma();
                }

                needComma = true;
                this.Write(name);
            }

            if (!skipCloseParentheses)
            {
                this.WriteCloseParentheses();
            }
        }

        protected virtual void EmitTypeParameters(IEnumerable<TypeParameterDeclaration> declarations, AstNode context)
        {
            this.WriteOpenParentheses();
            bool needComma = false;

            foreach (var p in declarations)
            {
                this.Emitter.Validator.CheckIdentifier(p.Name, context);

                if (needComma)
                {
                    this.WriteComma();
                }

                needComma = true;
                this.Write(p.Name.Replace(Bridge.Translator.Emitter.FIX_ARGUMENT_NAME, ""));
            }

            this.WriteCloseParentheses();
        }
    }
}
