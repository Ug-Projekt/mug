using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser
{
    public class NamespaceNode : IStatement
    {
        public NodeBuilder GlobalScope { get; set; }
        public MemberAccessNode Name { get; set; }
        public Range Position { get; set; }

        public NamespaceNode()
        {
            GlobalScope = new NodeBuilder();
        }
        public string Stringize(string indent = "")
        {
            return indent+$"NamespaceNode: {{\n{indent}   Name: {{\n{Name.Stringize(indent+"     ")}\n{indent}   }},\n{GlobalScope.Stringize(indent+"   ")}\n{indent}}}";
        }
    }
}
