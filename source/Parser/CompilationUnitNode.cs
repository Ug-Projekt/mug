using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser
{
    public class CompilationUnitNode : INode
    {
        public NodeBuilder GlobalScope { get; set; }
        public Range Position { get; set; }

        public CompilationUnitNode()
        {
            GlobalScope = new NodeBuilder();
        }
        public string Stringize(string indent = "")
        {
            return indent+"CompilationUnit:\n"+GlobalScope.Stringize(indent+"   ");
        }
    }
}
