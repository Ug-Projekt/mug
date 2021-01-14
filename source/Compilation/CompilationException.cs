﻿using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Compilation
{
    public class CompilationException : Exception
    {
        public MugLexer Lexer { get; }
        public Range BadToken { get; }
        public String Error { get; }
        public CompilationException(MugLexer lexer, Range bad, string message) : base(message)
        {
            Lexer = lexer;
            BadToken = bad;
            Error = message;
        }
    }
}
