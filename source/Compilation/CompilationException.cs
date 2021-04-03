using Mug.Models.Lexer;
using System;
using System.Collections.Generic;

namespace Mug.Compilation
{
    public class CompilationException : Exception
    {
        public List<MugError> DiagnosticBag { get; }
        public MugLexer Lexer { get; }
        public bool IsGlobalError
        {
            get
            {
                return DiagnosticBag is null;
            }
        }

        public CompilationException(List<MugError> diagnosticbag, MugLexer lexer) : this("Cannot build due to previous errors", lexer)
        {
            DiagnosticBag = diagnosticbag;
        }

        public CompilationException(string error, MugLexer lexer = null) : base(error)
        {
            Lexer = lexer;
        }
    }
}
