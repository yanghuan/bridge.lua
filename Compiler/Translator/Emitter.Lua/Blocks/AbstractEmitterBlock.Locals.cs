using Bridge.Contract;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;

namespace Bridge.Translator.Lua
{
    public static class LuaHelper {
        public const string Root = Emitter.ROOT;
        public const string Nil = "nil";
        public const string MergeVar = "t";
        public const string MutilNew = "new";
        public const string Typeof = Root + ".typeof";

        public static string Ident(this string s) {
            return "__" + s + "__";
        }

        public static string F(this string s, object obj0) {
            return string.Format(s, obj0);
        }

        public static string F(this string s,  object obj0, object obj1) {
            return string.Format(s, obj0, obj1);
        }

        public static string F(this string s, params object[] args) {
            return string.Format(s, args);
        }

        public static bool IsSingleExpression(this AstNode node) {
            var exp = node;
            bool noPrev = exp.PrevSibling == null;
            bool noNext = exp.NextSibling == null || exp.NextSibling.Role == Roles.Semicolon;
            return noPrev && noNext;
        }

        private static bool IsOverridable(IMember member) {
            return member.IsOverridable && !((DefaultResolvedTypeDefinition)member.DeclaringType).IsSealed;
        }

        public static bool IsInternalMember(this IMember member) {
            return member.DeclaringType == TransformCtx.CurClass && !IsOverridable(member);
        }

        public static IEnumerable<T[]> Cut<T>(this IList<T> list, int cellCount) {
            int index = 0;
            while(index < list.Count) {
                T[] cells = list.Skip(index).Take(cellCount).ToArray();
                yield return cells;
                index += cells.Length;
            }
        }

        public static bool IsCompileTimeConstantToString(this MemberResolveResult result) {
            return result.TargetResult.IsCompileTimeConstant && result.Member.Name == "ToString";
        }
    }

    public partial class AbstractEmitterBlock
    {
        public virtual void PushLocals()
        {
            if (this.Emitter.LocalsStack == null)
            {
                this.Emitter.LocalsStack = new Stack<Dictionary<string, AstType>>();
            }

            // Pushes even if null, else it will have nothing to pull later and another test will be needed.
            this.Emitter.LocalsStack.Push(this.Emitter.Locals);

            if (this.Emitter.Locals != null)
            {
                this.Emitter.Locals = new Dictionary<string, AstType>(this.Emitter.Locals);
            }
            else
            {
                this.Emitter.Locals = new Dictionary<string, AstType>();
            }
        }

        public virtual void PopLocals()
        {
            this.Emitter.Locals = this.Emitter.LocalsStack.Pop();
        }

        public virtual void ResetLocals() {
            this.Emitter.TempVariables = new Dictionary<string, bool>();
            this.Emitter.Locals = new Dictionary<string, AstType>();
            this.Emitter.IteratorCount = 0;
        }

        public virtual void AddLocals(IEnumerable<ParameterDeclaration> declarations, AstNode statement)
        {
            foreach(var item in declarations) {
                var name = this.Emitter.GetEntityName(item);
                var vName = this.AddLocal(item.Name, item.Type, name);
                this.Emitter.LocalsMap[item.Name] = vName;
            }
        }

        public string AddLocal(string name, AstType type, string valueName = null)
        {
           this.Emitter.Locals.Add(name, type);

            name = name.StartsWith(Bridge.Translator.Emitter.FIX_ARGUMENT_NAME) ? name.Substring(Bridge.Translator.Emitter.FIX_ARGUMENT_NAME.Length) : name;
            string vName = valueName ?? name;

            if (Helpers.IsReservedWord(vName))
            {
                vName = this.GetUniqueName(vName);
            }

            if (!this.Emitter.LocalsNamesMap.ContainsKey(name))
            {
                this.Emitter.LocalsNamesMap.Add(name, vName);
            }
            else
            {
                this.Emitter.LocalsNamesMap[name] = this.GetUniqueName(vName);
            }

            var result = this.Emitter.LocalsNamesMap[name];

            if (this.Emitter.IsAsync && !this.Emitter.AsyncVariables.Contains(result))
            {
                this.Emitter.AsyncVariables.Add(result);
            }

            return result;
        }

        protected virtual string GetUniqueName(string name)
        {
            int index = 1;
            if (this.Emitter.LocalsNamesMap.ContainsKey(name))
            {
                var value = this.Emitter.LocalsNamesMap[name];
                if (value.Length > name.Length)
                {
                    var suffix = value.Substring(name.Length);
                    int subindex;
                    bool isNumeric = int.TryParse(suffix, out subindex);
                    if (isNumeric)
                    {
                        index = subindex + 1;
                    }
                }
            }
            string tempName = name + index;
            while (this.Emitter.LocalsNamesMap.ContainsValue(tempName))
            {
                tempName = name + ++index;
            }
            return tempName;
        }

        protected void CheckConflictName(ref string name) {
            var namesMap = this.Emitter.LocalsNamesMap;
            if(namesMap != null && namesMap.ContainsKey(name)) {
                name = this.GetUniqueName(name);
            }
        }

        public virtual Dictionary<string, string> BuildLocalsMap()
        {
            var prevMap = this.Emitter.LocalsMap;

            if (prevMap == null)
            {
                this.Emitter.LocalsMap = new Dictionary<string, string>();
            }
            else
            {
                this.Emitter.LocalsMap = new Dictionary<string, string>(prevMap);
            }

            return prevMap;
        }

        public virtual void ClearLocalsMap(Dictionary<string, string> prevMap = null)
        {
            this.Emitter.LocalsMap = prevMap;
        }

        public virtual Dictionary<string, string> BuildLocalsNamesMap()
        {
            var prevMap = this.Emitter.LocalsNamesMap;

            if (prevMap == null)
            {
                this.Emitter.LocalsNamesMap = new Dictionary<string, string>();
            }
            else
            {
                this.Emitter.LocalsNamesMap = new Dictionary<string, string>(prevMap);
            }

            return prevMap;
        }

        public virtual void ClearLocalsNamesMap(Dictionary<string, string> prevMap = null)
        {
            this.Emitter.LocalsNamesMap = prevMap;
        }

        public virtual void ConvertParamsToReferences(IEnumerable<ParameterDeclaration> declarations)
        {
            if (declarations.Any())
            {
                var p = declarations.First().Parent;
                if (p != null)
                {
                    var rr = this.Emitter.Resolver.ResolveNode(p, this.Emitter) as MemberResolveResult;
                    if (rr != null)
                    {
                        var method = rr.Member as DefaultResolvedMethod;
                        if (method != null)
                        {
                            foreach (var prm in method.Parameters)
                            {
                                if (prm.IsOptional)
                                {
                                    if (prm.ConstantValue == null && (prm.Type.Kind == TypeKind.Struct || prm.Type.Kind == TypeKind.TypeParameter) && !prm.Type.IsKnownType(KnownTypeCode.NullableOfT))
                                    {
                                        this.Write(string.Format("if {0} == nil then {0} = ", prm.Name));
                                        this.Write(Inspector.GetStructDefaultValue(prm.Type, this.Emitter));
                                        this.Write(" end");
                                        this.WriteNewLine();
                                    }
                                    else if(prm.ConstantValue != null)
                                    {
                                        this.Write(string.Format("if {0} == nil then {0} = ", prm.Name));
                                        this.WriteScript(prm.ConstantValue);
                                        this.Write(" end");
                                        this.WriteNewLine();
                                    }
                                }
                                else if (prm.IsParams)
                                {
                                    string typeName = BridgeTypes.ToJsName(((ArrayType)prm.Type).ElementType, this.Emitter);
                                    this.Write(string.Format("if {0} == nil then {0} = System.Array.Empty({1}) end", prm.Name, typeName));
                                    this.WriteNewLine();
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual void IntroduceTempVar(string name)
        {
            this.Emitter.TempVariables[name] = true;
            if (this.Emitter.IsAsync && !this.Emitter.AsyncVariables.Contains(name))
            {
                this.Emitter.AsyncVariables.Add(name);
            }
        }

        protected virtual void RemoveTempVar(string name)
        {
            this.Emitter.TempVariables.Remove(name);
        }

        protected virtual string GetTempVarName()
        {
            foreach (var pair in this.Emitter.TempVariables)
            {
                if (!pair.Value)
                {
                    this.Emitter.TempVariables[pair.Key] = true;
                    return pair.Key;
                }
            }
            string baseNmae = "t".Ident();

            string name = baseNmae;
            int i = 0;

            while (this.Emitter.TempVariables.ContainsKey(name) || (this.Emitter.ParentTempVariables != null && this.Emitter.ParentTempVariables.ContainsKey(name)))
            {
                name = baseNmae + ++i;
            }

            name = baseNmae + (i > 0 ? i.ToString() : "");

            this.IntroduceTempVar(name);

            return name;
        }

        protected virtual void EmitTempVars(int pos, bool skipIndent = false)
        {
            if (this.Emitter.TempVariables.Count > 0)
            {
                var newLine = this.Emitter.IsNewLine;
                string temp = this.Emitter.Output.ToString(pos, this.Emitter.Output.Length - pos);
                this.Emitter.Output.Length = pos;
                this.Emitter.IsNewLine = true;

                List<string> assignments = new List<string>();
                List<string> vars = new List<string>();
                foreach(var localVar in this.Emitter.TempVariables) {
                    string name = localVar.Key;
                    if(name.IndexOf('=') != -1) {
                        assignments.Add(name);
                    }
                    else {
                        vars.Add(name);
                    }
                }

                if(vars.Count > 0) {
                    this.WriteVar(true);
                    foreach(string name in vars) {
                        this.EnsureComma(false);
                        this.Write(name);
                        this.Emitter.Comma = true;
                    }

                    if(!skipIndent) {
                        this.Indent();
                        this.WriteIndent();
                    }


                    this.Emitter.Comma = false;
                    this.WriteSemiColon();
                    this.Outdent();
                    this.WriteNewLine();
                }

                if(assignments.Count > 0) {
                    foreach(string name in assignments) {
                        this.Write("local ", name, " ");
                    }
                    this.WriteNewLine();
                }

                this.Emitter.Output.Append(temp);
                this.Emitter.IsNewLine = newLine;
            }
        }
    }
}
