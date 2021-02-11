using System;
using System.Collections.Generic;

namespace Mug.Models.Parser
{
    public class NodeBuilder : INode
    {
        private readonly List<INode> nodes = new();
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
    }
}
