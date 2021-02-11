using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class FieldNode : INode
    {
        public String Name { get; set; }
        public MugType Type { get; set; }
        public Modifier Modifier { get; set; }
        public Range Position { get; set; }
    }
}
