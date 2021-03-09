using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class NamespaceNode : INode
    {
        public string NodeKind => "Namespace";
        public NodeBuilder Members { get; set; }
        public INode Name { get; set; }
        public Range Position { get; set; }

        public NamespaceNode()
        {
            Members = new NodeBuilder();
        }
    }
}
