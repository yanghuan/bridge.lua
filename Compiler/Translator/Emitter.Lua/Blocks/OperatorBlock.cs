using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;

namespace Bridge.Translator.Lua
{
    public class OperatorBlock : AbstractMethodBlock
    {
        public OperatorBlock(IEmitter emitter, OperatorDeclaration operatorDeclaration)
            : base(emitter, operatorDeclaration)
        {
            this.Emitter = emitter;
            this.OperatorDeclaration = operatorDeclaration;
        }

        public OperatorDeclaration OperatorDeclaration
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            this.EmitOperatorDeclaration(this.OperatorDeclaration);
        }

        protected void EmitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
        {
            foreach (var attrSection in operatorDeclaration.Attributes)
            {
                foreach (var attr in attrSection.Attributes)
                {
                    var rr = this.Emitter.Resolver.ResolveNode(attr.Type, this.Emitter);
                    if (rr.Type.FullName == "Bridge.ExternalAttribute" || rr.Type.FullName == "Bridge.IgnoreAttribute")
                    {
                        return;
                    }
                }
            }

            XmlToJsDoc.EmitComment(this, operatorDeclaration);
            this.EnsureNewLine();
            this.ResetLocals();
            var prevMap = this.BuildLocalsMap();
            var prevNamesMap = this.BuildLocalsNamesMap();
            this.AddLocals(operatorDeclaration.Parameters, operatorDeclaration.Body);

            var typeDef = this.Emitter.GetTypeDefinition();
            var overloads = OverloadsCollection.Create(this.Emitter, operatorDeclaration);

            string name;
            if (overloads.HasOverloads)
            {
                name = overloads.GetOverloadName();
            }
            else
            {
                name = this.Emitter.GetEntityName(operatorDeclaration);
            }
            string newName = Helpers.GetOperatorMapping(name);
            if(newName != null) {
                name = newName;
            }

            TransformCtx.CurClassMethodNames.Add(new TransformCtx.MethodInfo() { Name = name });

            this.Write(name);
            this.WriteEqualsSign();
            this.WriteFunction();

            this.EmitMethodParameters(operatorDeclaration.Parameters, operatorDeclaration);
            this.WriteSpace();
            this.BeginFunctionBlock();
            MarkTempVars();
            operatorDeclaration.Body.AcceptVisitor(this.Emitter);
            EmitTempVars();
            this.EndFunctionBlock();

            this.ClearLocalsMap(prevMap);
            this.ClearLocalsNamesMap(prevNamesMap);
            this.Emitter.Comma = true;
        }
    }
}