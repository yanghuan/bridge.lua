using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;

using Bridge.Contract;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

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
            CheckInterfacePeopertys();
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

        private void CheckInterfacePeopertys() {
            foreach(ITypeInfo type in Types) {
                if(type.ClassType == ClassType.Class && !type.IsStatic) {
                    foreach(var baseInterface in type.Type.GetAllBaseTypeDefinitions()) {
                        if(baseInterface.Kind == TypeKind.Interface) {
                            foreach(DefaultResolvedProperty property in baseInterface.Properties) {
                                CheckInterfacePeoperty(type, property);
                            }
                        }
                    }
                }
            }
        }

        private void CheckInterfacePeoperty(ITypeInfo type, DefaultResolvedProperty property) {
            if(!type.InstanceProperties.ContainsKey(property.Name)) {
                foreach(IType baseClass in type.Type.GetAllBaseTypes()) {
                    if(baseClass.Kind == TypeKind.Class) {
                        BridgeType baseBridgeType = BridgeTypes.Get(baseClass);
                        ITypeInfo baseTypeInfo = baseBridgeType.TypeInfo;
                        if(baseTypeInfo != null) {
                            if(baseTypeInfo.InstanceProperties.ContainsKey(property.Name)) {
                                var instanceConfig = baseTypeInfo.InstanceConfig;
                                int fieldIndex = instanceConfig.Fields.FindIndex(i => i.Name == property.Name);
                                if(fieldIndex != -1) {
                                    if(!instanceConfig.Properties.Exists(i => i.Name == property.Name)) {
                                        TypeConfigItem item = instanceConfig.Fields[fieldIndex];
                                        instanceConfig.Properties.Add(item);
                                        instanceConfig.Fields.RemoveAt(fieldIndex);

                                        TypeDefinition typeDefinition = baseBridgeType.TypeDefinition;
                                        PropertyDefinition propertyDefinition = typeDefinition.Properties.First(i => i.Name == property.Name);
                                        Helpers.SetCacheOfAutoPropertyOfDefinition(propertyDefinition);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
