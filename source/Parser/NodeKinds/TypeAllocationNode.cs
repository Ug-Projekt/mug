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
        private List<FieldAssignmentNode> _body { get; set; } = new();
        public FieldAssignmentNode[] Body
        {
            get
            {
                return _body.ToArray();
            }
            set
            {
                _body = value.ToList();
            }
        }
        public Range Position { get; set; }
        public void AddFieldAssign(FieldAssignmentNode fieldAssign)
        {
            _body.Add(fieldAssign);
        }
    }
}
