using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System.Collections.Generic;
using System.Linq;

namespace Bridge.Translator.Lua
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

            EntityDeclaration entityDeclaration = context as EntityDeclaration;
            if(entityDeclaration != null) {
                if(!entityDeclaration.HasModifier(Modifiers.Static)) {
                    this.WriteThis();
                    if(declarations.Any()) {
                        this.WriteComma();
                    }
                }
            }

            foreach (ParameterDeclaration p in declarations)
            {
                string name = p.Name;
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
        }

        private int beginPosition_;

        protected void MarkTempVars() {
            beginPosition_ = this.Emitter.Output.Length;
        }

        protected void EmitTempVars() {
            if(beginPosition_ == -1) {
                throw new System.NotSupportedException();
            }
            this.EmitTempVars(beginPosition_);
            beginPosition_ = -1;
        }
    }
}
