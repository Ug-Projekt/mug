using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser
{
    public interface INode
    {
        public abstract Range Position { get; set; }
        public abstract string Stringize(string indent = "");
    }
}
