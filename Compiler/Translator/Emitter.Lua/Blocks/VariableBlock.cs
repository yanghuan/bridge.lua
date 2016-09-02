using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator.Lua
{
    public class VariableBlock : AbstractEmitterBlock
    {
        public VariableBlock(IEmitter emitter, VariableDeclarationStatement variableDeclarationStatement)
            : base(emitter, variableDeclarationStatement)
        {
            this.Emitter = emitter;
            this.VariableDeclarationStatement = variableDeclarationStatement;
        }

        public VariableDeclarationStatement VariableDeclarationStatement
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            if(VariableDeclarationStatement.Modifiers != Modifiers.Const) {
                this.VisitVariableDeclarationStatement();
            }
        }

        protected virtual void VisitVariableDeclarationStatement()
        {
            bool needVar = true;
            bool needComma = false;
            bool addSemicolon = !this.Emitter.IsAsync;

            var oldSemiColon = this.Emitter.EnableSemicolon;
            var asyncExpressionHandling = this.Emitter.AsyncExpressionHandling;

            foreach (var variable in this.VariableDeclarationStatement.Variables)
            {
                var varName = this.AddLocal(variable.Name, this.VariableDeclarationStatement.Type);

                if (variable.Initializer != null && !variable.Initializer.IsNull && variable.Initializer.ToString().Contains(Bridge.Translator.Emitter.FIX_ARGUMENT_NAME))
                {
                    continue;
                }

                if (needVar)
                {
                    needVar = false;
                }

                this.WriteAwaiters(variable.Initializer);

                var hasInitializer = !variable.Initializer.IsNull;
                if (variable.Initializer.IsNull && !this.VariableDeclarationStatement.Type.IsVar())
                {
                    var typeDef = this.Emitter.GetTypeDefinition(this.VariableDeclarationStatement.Type, true);
                    if (typeDef != null && typeDef.IsValueType && !typeDef.IsEnum && !this.Emitter.Validator.IsIgnoreType(typeDef))
                    {
                        hasInitializer = true;
                    }
                }

                if (!this.Emitter.IsAsync || hasInitializer)
                {
                    if (needComma)
                    {
                        if (this.Emitter.IsAsync)
                        {
                            this.WriteSemiColon();
                        }
                        else
                        {
                            this.WriteSemiColon();
                            this.WriteSpace();
                        }
                    }

                    needComma = true;

                    this.WriteVar();
                    this.Write(varName);
                }

                if (hasInitializer)
                {
                    addSemicolon = true;
                    this.Write(" = ");

                    var oldValue = this.Emitter.ReplaceAwaiterByVar;
                    this.Emitter.ReplaceAwaiterByVar = true;

                    if (!variable.Initializer.IsNull)
                    {
                        variable.Initializer.AcceptVisitor(this.Emitter);
                    }
                    else
                    {
                        //throw new System.Exception("not reach!!");
                        string name = BridgeTypes.ToJsName(this.VariableDeclarationStatement.Type, this.Emitter);
                        var resolve = this.Emitter.Resolver.ResolveNode(variable, this.Emitter);
                        if(resolve.Type.Kind == ICSharpCode.NRefactory.TypeSystem.TypeKind.Struct) {
                            this.Write(name, "(1)");
                        }
                        else {
                            this.Write(name, "()");
                        }
                    }
                    this.Emitter.ReplaceAwaiterByVar = oldValue;
                }
            }

            this.Emitter.AsyncExpressionHandling = asyncExpressionHandling;
            if (this.Emitter.EnableSemicolon && !needVar && addSemicolon)
            {
                this.WriteSemiColon(true);
            }

            if (oldSemiColon)
            {
                this.Emitter.EnableSemicolon = true;
            }
        }
    }
}
