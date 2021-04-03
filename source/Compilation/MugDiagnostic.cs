﻿using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Compilation
{
    public class MugDiagnostic
    {
        private readonly List<MugError> _diagnostic = new();

        public int Count => _diagnostic.Count;

        public void Report(int pos, string error)
        {
            Report(pos..(pos + 1), error);
        }

        public void Report(Range position, string message)
        {
            Report(new MugError(position, message));
        }

        public void Report(MugError error)
        {
            if (!_diagnostic.Contains(error))
                _diagnostic.Add(error);
        }

        public void CheckDiagnostic(MugLexer lexer)
        {
            if (_diagnostic.Count > 0)
                throw new CompilationException(lexer);
        }

        public List<MugError> GetErrors()
        {
            return _diagnostic;
        }
    }
}
