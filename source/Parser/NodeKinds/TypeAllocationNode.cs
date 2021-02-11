using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class TypeAllocationNode : INode
    {
        public MugType Name { get; set; }
        List<FieldAssignmentNode> _body { get; set; } = new();
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
