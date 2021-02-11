using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class BlockNode : INode
    {
        public INode[] Statements
        {
            get
            {
                return statements.ToArray();
            }
        }

        public Range Position { get; set; }

        List<INode> statements = new();
        public void Add(INode node)
        {
            statements.Add(node);
        }
    }
}
