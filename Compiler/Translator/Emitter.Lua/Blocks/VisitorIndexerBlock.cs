using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator.Lua
{
    public class VisitorIndexerBlock : AbstractMethodBlock
    {
        public VisitorIndexerBlock(IEmitter emitter, IndexerDeclaration indexerDeclaration)
            : base(emitter, indexerDeclaration)
        {
            this.Emitter = emitter;
            this.IndexerDeclaration = indexerDeclaration;
        }

        public IndexerDeclaration IndexerDeclaration
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            this.EmitIndexerMethod(this.IndexerDeclaration, this.IndexerDeclaration.Getter, false);
            this.EmitIndexerMethod(this.IndexerDeclaration, this.IndexerDeclaration.Setter, true);
        }

        protected virtual void EmitIndexerMethod(IndexerDeclaration indexerDeclaration, Accessor accessor, bool setter)
        {
            if (!accessor.IsNull && this.Emitter.GetInline(accessor) == null)
            {
                this.EnsureNewLine();

                this.ResetLocals();

                var prevMap = this.BuildLocalsMap();
                var prevNamesMap = this.BuildLocalsNamesMap();

                if (setter)
                {
                    this.AddLocals(new ParameterDeclaration[] { new ParameterDeclaration { Name = "value" } }, accessor.Body);
                }

                XmlToJsDoc.EmitComment(this, this.IndexerDeclaration);
                var overloads = OverloadsCollection.Create(this.Emitter, indexerDeclaration, setter);

                string name = overloads.GetOverloadName();
                name = (setter ? "set" : "get") + name;
                TransformCtx.CurClassMethodNames.Add(new TransformCtx.MethodInfo() {
                    Name = name,
                    IsPrivate = accessor.HasModifier(Modifiers.Private),
                });

                this.Write(name);
                this.WriteEqualsSign();
                this.WriteFunction();
                this.EmitMethodParameters(indexerDeclaration.Parameters, indexerDeclaration, setter);

                if (setter)
                {
                    this.Write(", value)");
                }
                this.WriteSpace();

                var script = this.Emitter.GetScript(accessor);

                if (script == null)
                {
                    this.BeginFunctionBlock();
                    accessor.Body.AcceptVisitor(this.Emitter);
                    this.EndFunctionBlock();
                }
                else
                {
                    this.BeginFunctionBlock();

                    foreach (var line in script)
                    {
                        this.Write(line);
                        this.WriteNewLine();
                    }

                    this.EndFunctionBlock();
                }

                this.ClearLocalsMap(prevMap);
                this.ClearLocalsNamesMap(prevNamesMap);
                this.Emitter.Comma = true;
            }
        }
    }
}
