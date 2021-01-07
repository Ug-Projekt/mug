using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser
{
    public class NodeBuilder : INode
    {
        List<INode> nodes = new();
        public INode[] Nodes
        {
            get
            {
                return nodes.ToArray();
            }
        }
        public Range Position { get; set; }

        public void Add(INode node)
        {
            nodes.Add(node);
        }
        public string Stringize(string indent = "")
        {
            string nodes = "";
            for (int i = 0; i < this.nodes.Count; i++)
                nodes += this.nodes[i].Stringize(indent + "   ")+",\n";
            return indent + $"NodeBuilder: {{\n{nodes}{indent}}}";
        }
    }
}
