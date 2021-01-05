using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser
{
    public struct CompilationUnitNode : INode
    {
        public NodeBuilder GlobalScope { get; set; }
        public CompilationUnitNode(NodeBuilder globalScope)
        {
            GlobalScope = globalScope;
        }
        public string Stringize(string indent = "")
        {
            return indent+"CompilationUnit:\n"+GlobalScope.Stringize(indent+"   ");
        }
    }
}
