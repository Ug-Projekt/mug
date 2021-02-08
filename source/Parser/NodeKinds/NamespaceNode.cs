using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class NamespaceNode : INode
    {
        public NodeBuilder Members { get; set; }
        public INode Name { get; set; }
        public Range Position { get; set; }

        public NamespaceNode()
        {
            Members = new NodeBuilder();
        }
        public string Dump(string indent = "")
        {
            return indent+$"NamespaceNode: {{\n{indent}   Name: {{\n{Name.Dump(indent+"     ")}\n{indent}   }},\n{Members.Dump(indent+"   ")}\n{indent}}}";
        }
    }
}
