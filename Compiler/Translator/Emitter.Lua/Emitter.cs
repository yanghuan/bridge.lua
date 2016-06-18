using Bridge.Contract;
using Mono.Cecil;
using System.Collections.Generic;

namespace Bridge.Translator.Lua
{
    public partial class Emitter : Visitor, IEmitter
    {
        public Emitter(IDictionary<string,
            TypeDefinition> typeDefinitions,
            BridgeTypes bridgeTypes,
            List<ITypeInfo> types,
            IValidator validator,
            IMemberResolver resolver,
            Dictionary<string, ITypeInfo> typeInfoDefinitions,
            ILogger logger)
        {
            this.Log = logger;

            this.Resolver = resolver;
            this.TypeDefinitions = typeDefinitions;
            this.TypeInfoDefinitions = typeInfoDefinitions;
            this.Types = types;
            this.BridgeTypes = bridgeTypes;
            this.BridgeTypes.InitItems(this);
            this.Types.Sort(this.CompareTypeInfosByName);
            this.SortTypesByInheritance();
            this.Validator = validator;
            this.AssignmentType = ICSharpCode.NRefactory.CSharp.AssignmentOperatorType.Any;
            this.UnaryOperatorType = ICSharpCode.NRefactory.CSharp.UnaryOperatorType.Any;
            this.JsDoc = new JsDoc();
        }

        public virtual Dictionary<string, string> Emit()
        {
            var blocks = this.GetBlocks();
            foreach (var block in blocks)
            {
                this.JsDoc.Init();
                block.Emit();
            }

            return this.TransformOutputs();
        }

        private IEnumerable<IAbstractEmitterBlock> GetBlocks()
        {
            yield return new EmitBlock(this);
        }
    }
}
