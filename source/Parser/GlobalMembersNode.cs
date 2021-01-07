using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser
{
    public class GlobalMembersNode : INode
    {
        public NodeBuilder GlobalScope { get; set; }
        public Range Position { get; set; }

        public GlobalMembersNode()
        {
            GlobalScope = new NodeBuilder();
        }
        public string Stringize(string indent = "")
        {
            return indent+$"GlobalMembersNode: {{\n{GlobalScope.Stringize(indent+"   ")}\n{indent}}}";
        }
    }
}
