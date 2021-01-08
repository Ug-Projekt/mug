using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class ForCounterReference : INode
    {
        public String ReferenceName { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+$"ForCounterReference: {ReferenceName}";
        }
    }
}
