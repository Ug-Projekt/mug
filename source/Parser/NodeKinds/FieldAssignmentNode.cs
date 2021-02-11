using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class FieldAssignmentNode : INode
    {
        public String Name { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }
    }
}
