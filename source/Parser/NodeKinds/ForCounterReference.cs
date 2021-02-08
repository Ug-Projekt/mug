using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class ForCounterReference : INode
    {
        public INode ReferenceName { get; set; }
        public Range Position { get; set; }

        public string Dump(string indent = "")
        {
            return indent+$"ForCounterReference: {ReferenceName}";
        }
    }
}
