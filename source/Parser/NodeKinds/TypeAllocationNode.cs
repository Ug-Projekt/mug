using Mug.TypeSystem;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser.NodeKinds
{
    public class TypeAllocationNode : INode
    {
        public MugType Name { get; set; }
        private List<FieldAssignmentNode> _body { get; set; } = new();
        public FieldAssignmentNode[] Body
        {
            get
            {
                return _body.ToArray();
            }
        }
        public Range Position { get; set; }
        public void AddFieldAssign(FieldAssignmentNode fieldAssign)
        {
            _body.Add(fieldAssign);
        }
    }
}
