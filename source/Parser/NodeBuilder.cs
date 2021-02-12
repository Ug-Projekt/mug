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
        public int Lenght
        {
            get
            {
                return nodes.Count;
            }
        }

        public void Add(INode node)
        {
            nodes.Add(node);
        }
    }
}
