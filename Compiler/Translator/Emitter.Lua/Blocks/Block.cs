using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using System.Linq;

namespace Bridge.Translator.Lua
{
    public class Block : AbstractEmitterBlock
    {
        public Block(IEmitter emitter, BlockStatement blockStatement)
            : base(emitter, blockStatement)
        {
            this.Emitter = emitter;
            this.BlockStatement = blockStatement;

            if (this.Emitter.IgnoreBlock == blockStatement)
            {
                this.AsyncNoBraces = true;
            }

            if (this.Emitter.NoBraceBlock == blockStatement)
            {
                this.NoBraces = true;
            }
        }

        public BlockStatement BlockStatement
        {
            get;
            set;
        }

        protected bool AddEndBlock
        {
            get;
            set;
        }

        public bool AsyncNoBraces
        {
            get;
            set;
        }

        public bool NoBraces
        {
            get;
            set;
        }

        public int BeginPosition
        {
            get;
            set;
        }

        public bool IsMethodBlock
        {
            get;
            set;
        }

        public bool IsYield
        {
            get;
            set;
        }

        public IType ReturnType
        {
            get;
            set;
        }

        private IType OldReturnType
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            this.EmitBlock();
        }

        public void EmitBlock()
        {
            this.BeginEmitBlock();
            this.DoEmitBlock();
            this.EndEmitBlock();
        }

        public void DoEmitBlock()
        {
            if (this.BlockStatement.Parent is MethodDeclaration)
            {
                this.IsMethodBlock = true;
                var methodDeclaration = (MethodDeclaration)this.BlockStatement.Parent;
                if (!methodDeclaration.ReturnType.IsNull)
                {
                    var rr = this.Emitter.Resolver.ResolveNode(methodDeclaration.ReturnType, this.Emitter);
                    this.ReturnType = rr.Type;
                }
                this.ConvertParamsToReferences(methodDeclaration.Parameters);
            }
            else if (this.BlockStatement.Parent is AnonymousMethodExpression)
            {
                this.IsMethodBlock = true;
                var methodDeclaration = (AnonymousMethodExpression)this.BlockStatement.Parent;
                var rr = this.Emitter.Resolver.ResolveNode(methodDeclaration, this.Emitter);
                this.ReturnType = rr.Type;
                this.ConvertParamsToReferences(methodDeclaration.Parameters);
            }
            else if (this.BlockStatement.Parent is LambdaExpression)
            {
                this.IsMethodBlock = true;
                var methodDeclaration = (LambdaExpression)this.BlockStatement.Parent;
                var rr = this.Emitter.Resolver.ResolveNode(methodDeclaration, this.Emitter);
                this.ReturnType = rr.Type;
                this.ConvertParamsToReferences(methodDeclaration.Parameters);
            }
            else if (this.BlockStatement.Parent is ConstructorDeclaration)
            {
                this.IsMethodBlock = true;
                this.ConvertParamsToReferences(((ConstructorDeclaration)this.BlockStatement.Parent).Parameters);
            }
            else if (this.BlockStatement.Parent is OperatorDeclaration)
            {
                this.IsMethodBlock = true;
                this.ConvertParamsToReferences(((OperatorDeclaration)this.BlockStatement.Parent).Parameters);
            }
            else if (this.BlockStatement.Parent is Accessor)
            {
                this.IsMethodBlock = true;
                var role = this.BlockStatement.Parent.Role.ToString();

                if (role == "Setter")
                {
                    this.ConvertParamsToReferences(new ParameterDeclaration[] { new ParameterDeclaration { Name = "value" } });
                }
                else if (role == "Getter")
                {
                    var methodDeclaration = (Accessor)this.BlockStatement.Parent;
                    if (!methodDeclaration.ReturnType.IsNull)
                    {
                        var rr = this.Emitter.Resolver.ResolveNode(methodDeclaration.ReturnType, this.Emitter);
                        this.ReturnType = rr.Type;
                    }
                }
            }

            if (this.IsMethodBlock && YieldBlock.HasYield(this.BlockStatement))
            {
                this.IsYield = true;
                YieldBlock.EmitYield(this, this.ReturnType);
            }

            if (this.IsMethodBlock)
            {
                this.OldReturnType = this.Emitter.ReturnType;
                this.Emitter.ReturnType = this.ReturnType;
            }

            if (this.Emitter.BeforeBlock != null)
            {
                this.Emitter.BeforeBlock();
                this.Emitter.BeforeBlock = null;
            }

            foreach(var statement in this.BlockStatement.Children) {
                bool isBlockStatement = statement is BlockStatement;
                if(isBlockStatement) {
                    this.BeginDoBlock();
                }
                statement.AcceptVisitor(this.Emitter);
                if(isBlockStatement) {
                    this.EndCodeBlock();
                    this.WriteNewLine();
                }
            }
        }

        public void EndEmitBlock()
        {
            if (this.IsYield)
            {
                YieldBlock.EmitYieldReturn(this, this.ReturnType);
            }

            if (this.IsMethodBlock)
            {
                this.Emitter.ReturnType = this.OldReturnType;
            }

            if (this.AddEndBlock)
            {
                this.WriteNewLine();
                this.EndBlock();
            }

            if (this.IsMethodBlock && !this.Emitter.IsAsync)
            {
                this.EmitTempVars(this.BeginPosition);
            }

            this.PopLocals();
        }

        public void BeginEmitBlock()
        {
            this.PushLocals();
            this.BeginPosition = this.Emitter.Output.Length;
        }
    }
}
