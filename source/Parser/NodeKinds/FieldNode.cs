using Mug.TypeSystem;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class FieldNode : INode
    {
        public string NodeKind => "Field";
        public string Name { get; set; }
        public MugType Type { get; set; }
        public Range Position { get; set; }
    }
}
