using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Compilation
{
    public class MugError
    {
        public Range Bad { get; }
        public string Message { get; }
        public int LineAt(string source)
        {
            return CompilationErrors.CountLines(source, Bad.Start.Value) - 1;
        }

        public MugError(Range position, string message)
        {
            Bad = position;
            Message = message;
        }
    }
}
