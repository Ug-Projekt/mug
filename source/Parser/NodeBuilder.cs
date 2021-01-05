using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser
{
    public class NodeBuilder : INode
    {
        List<INode> Nodes = new();
        public void Add(INode node)
        {
            Nodes.Add(node);
        }
        public string Stringize(string indent = "")
        {
            string nodes = "";
            for (int i = 0; i < Nodes.Count; i++)
                nodes += Nodes[i].Stringize(indent+"   ")+'\n';
            return indent + "NodeBuilder:\n" + nodes;
        }
    }
}
