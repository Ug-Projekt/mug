using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mug.Models.Parser.NodeKinds
{
    public class TypeAllocationNode : INode
    {
        public string NodeKind => "StructAllocation";
        public MugType Name { get; set; }
        public List<MugType> Generics { get; set; } = new();
        public List<FieldAssignmentNode> Body { get; set; } = new();
        public Range Position { get; set; }
    }
}
