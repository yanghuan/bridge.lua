using System;
using System.Collections.Generic;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using Bridge.Contract;

namespace Bridge.Translator
{
    public class ParsedSourceFile
    {
        public ParsedSourceFile(SyntaxTree syntaxTree, CSharpUnresolvedFile parsedFile)
        {
            SyntaxTree = syntaxTree;
            ParsedFile = parsedFile;
        }

        public SyntaxTree SyntaxTree
        {
            get;
            private set;
        }

        public CSharpUnresolvedFile ParsedFile
        {
            get;
            private set;
        }

        public string FileName {
            get {
                return ParsedFile.FileName;
            }
        }
    }
}
