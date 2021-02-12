using Mug.Models.Lexer;
using System;

namespace Mug.Compilation
{
    public class CompilationException : Exception
    {
        public MugLexer Lexer { get; }
        public Range Bad { get; }
        public int LineAt
        {
            get
            {
                return CompilationErrors.CountLines(Lexer.Source, Bad.Start.Value);
            }
        }
        public CompilationException(MugLexer lexer, Range bad, string message) : base(message)
        {
            Lexer = lexer;
            Bad = bad;
        }
    }
}
