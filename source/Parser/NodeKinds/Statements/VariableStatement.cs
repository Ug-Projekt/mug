using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct VariableStatement : INode
    {
        public String Name { get; set; }
        public MugType Type { get; set; }
        public Boolean IsAssigned { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }
    }
}
