using Mug.TypeSystem;
using System;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct VariableStatement : INode
    {
        public string NodeKind => "Var";
        public string Name { get; set; }
        public MugType Type { get; set; }
        public bool IsAssigned { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }
    }
}
