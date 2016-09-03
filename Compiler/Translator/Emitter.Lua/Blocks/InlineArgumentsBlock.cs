using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bridge.Translator.Lua
{
    public class InlineArgumentsBlock : AbstractEmitterBlock
    {
        public InlineArgumentsBlock(IEmitter emitter, ArgumentsInfo argsInfo, string inline)
            : base(emitter, argsInfo.Expression)
        {
            this.Emitter = emitter;
            this.ArgumentsInfo = argsInfo;
            this.InlineCode = inline;

            argsInfo.AddExtensionParam();
        }

        public ArgumentsInfo ArgumentsInfo
        {
            get;
            set;
        }

        public string InlineCode
        {
            get;
            set;
        }

        protected override void DoEmit()
        {
            this.EmitInlineExpressionList(this.ArgumentsInfo, this.InlineCode);
        }

        private static Regex _formatArg = new Regex(@"(,?\s*)\{(\*?)([\w|^]+)(\:(\w+))?\}");

        protected virtual IList<Expression> GetExpressionsByKey(IEnumerable<NamedParamExpression> expressions, string key)
        {
            if (expressions == null)
            {
                return new List<Expression>();
            }

            if (Regex.IsMatch(key, "^\\d+$"))
            {
                var list = new List<Expression>();
                int index = int.Parse(key);
                var e = expressions.ElementAtOrDefault(index);
                if(e != null) {
                    list.Add(e.Expression);
                }
                return list;
            }

            return expressions.Where(e => e.Name == key).Select(e => e.Expression).ToList();
        }

        protected virtual AstType GetAstTypeByKey(IEnumerable<TypeParamExpression> types, string key)
        {
            if(key[0] == '^') {
                int index = int.Parse(key.Substring(1));
                var e = types.ElementAtOrDefault(index);
                return e != null ? e.AstType : null;
            }
            return types.Where(e => e.Name == key && e.AstType != null).Select(e => e.AstType).FirstOrDefault();
        }

        protected virtual IType GetITypeByKey(IEnumerable<TypeParamExpression> types, string key)
        {
            if(key[0] == '^') {
                int index = int.Parse(key.Substring(1));
                var e = types.ElementAtOrDefault(index);
                return e != null ? e.IType : null;
            }
            return types.Where(e => e.Name == key && e.IType != null).Select(e => e.IType).FirstOrDefault();
        }

        public static string ReplaceInlineArgs(AbstractEmitterBlock block, string inline, Expression[] args)
        {
            var emitter = block.Emitter;
            inline = _formatArg.Replace(inline, delegate(Match m)
            {
                int count = emitter.Writers.Count;
                string separate = m.Groups[1].Value;
                string key = m.Groups[3].Value;
                string modifier = m.Groups[2].Success ? m.Groups[5].Value : null;

                StringBuilder oldSb = emitter.Output;
                emitter.Output = new StringBuilder();

                Expression expr = null;

                if (Regex.IsMatch(key, "^\\d+$"))
                {
                    expr = args.Skip(int.Parse(key)).FirstOrDefault();
                }
                else
                {
                    expr = args.FirstOrDefault(e => e.ToString() == key);
                }

                string s = "";
                if (expr != null)
                {
                    var writer = block.SaveWriter();
                    block.NewWriter();
                    expr.AcceptVisitor(emitter);
                    s = emitter.Output.ToString();
                    block.RestoreWriter(writer);

                    if (modifier == "raw")
                    {
                        s = s.Trim('"');
                    }
                }

                block.Write(block.WriteIndentToString(s));

                if (emitter.Writers.Count != count)
                {
                    block.PopWriter();
                }

                string replacement = emitter.Output.ToString();
                emitter.Output = oldSb;

                return replacement;
            });

            return inline;
        }

        protected virtual void EmitInlineExpressionList(ArgumentsInfo argsInfo, string inline)
        {
            IEnumerable<NamedParamExpression> expressions = argsInfo.NamedExpressions;
            IEnumerable<TypeParamExpression> typeParams = argsInfo.TypeArguments;

            this.Write("");
            inline = _formatArg.Replace(inline, delegate(Match m)
            {
                int count = this.Emitter.Writers.Count;
                string separate = m.Groups[1].Value;
                string key = m.Groups[3].Value;
                bool isRaw = m.Groups[2].Success && m.Groups[2].Value == "*";
                bool ignoreArray = isRaw || argsInfo.ParamsExpression == null;
                string modifier = m.Groups[2].Success ? m.Groups[5].Value : null;
                string paramsName = null;
                if (argsInfo.ResolveResult != null)
                {
                    var paramsParam = argsInfo.ResolveResult.Member.Parameters.FirstOrDefault(p => p.IsParams);
                    if (paramsParam != null)
                    {
                        paramsName = paramsParam.Name;
                    }
                }

                if (modifier == "array")
                {
                    ignoreArray = false;
                }

                StringBuilder oldSb = this.Emitter.Output;
                this.Emitter.Output = new StringBuilder();

                if (key == "this" || key == argsInfo.ThisName || (key == "0" && argsInfo.IsExtensionMethod))
                {
                    string thisValue = argsInfo.GetThisValue();
                    if (thisValue != null)
                    {
                        this.Write(thisValue);
                    }
                }
                else if(key == "class") {
                    var resolverResult = (MemberResolveResult)this.Emitter.Resolver.ResolveNode(argsInfo.Expression, this.Emitter);
                    string typeName = BridgeTypes.ToJsName(resolverResult.TargetResult.Type, this.Emitter);
                    this.Write(typeName);
                }
                else
                {
                    IList<Expression> exprs = this.GetExpressionsByKey(expressions, key);
                    if (exprs.Count > 0)
                    {
                        if (exprs.Count > 1 || paramsName == key)
                        {
                            if (exprs.Count == 1 && exprs[0] != null && exprs[0].Parent != null)
                            {
                                var exprrr = this.Emitter.Resolver.ResolveNode(exprs[0], this.Emitter);
                                if (exprrr.Type.Kind == TypeKind.Array)
                                {
                                    ignoreArray = true;
                                }
                            }

                            if (!ignoreArray)
                            {
                                this.Write("[");
                            }

                            if (exprs.Count == 1 && exprs[0] == null)
                            {
                                this.Write("null");
                            }
                            else
                            {
                                new ExpressionListBlock(this.Emitter, exprs, null).Emit();    
                            }

                            if (!ignoreArray)
                            {
                                this.Write("]");
                            }
                        }
                        else
                        {
                            string s;
                            if (exprs[0] != null)
                            {
                                var writer = this.SaveWriter();
                                this.NewWriter();
                                exprs[0].AcceptVisitor(this.Emitter);
                                s = this.Emitter.Output.ToString();
                                this.RestoreWriter(writer);

                                if (modifier == "raw")
                                {
                                    s = s.Trim('"');
                                }
                            }
                            else
                            {
                                s = "null";
                            }

                            this.Write(this.WriteIndentToString(s));
                        }
                    }
                    else if (typeParams != null)
                    {
                        var type = this.GetAstTypeByKey(typeParams, key);
                        if (type != null)
                        {
                            type.AcceptVisitor(this.Emitter);
                        }
                        else
                        {
                            var iType = this.GetITypeByKey(typeParams, key);
                            if (iType != null)
                            {
                                new CastBlock(this.Emitter, iType).Emit();
                            }
                        }
                    }
                }

                if (this.Emitter.Writers.Count != count)
                {
                    this.PopWriter();
                }

                string replacement = this.Emitter.Output.ToString();
                this.Emitter.Output = oldSb;
                if(!string.IsNullOrEmpty(replacement)) {
                    replacement = separate + replacement;
                }
                return replacement;
            });

            this.Write(inline);
        }
    }
}
