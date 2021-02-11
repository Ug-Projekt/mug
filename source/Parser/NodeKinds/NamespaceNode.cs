using Newtonsoft.Json;
using System;

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
        public string Dump()
        {
            return JsonConvert.SerializeObject(Members.Nodes, Formatting.Indented);
        }
    }
}
