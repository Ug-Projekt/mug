using Mug.TypeSystem;
using System;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct VariableStatement : INode
    {
        public string NodeKind => "Var";
        public String Name { get; set; }
        public MugType Type { get; set; }
        public Boolean IsAssigned { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }
    }
}
