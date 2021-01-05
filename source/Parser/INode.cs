using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser
{
    public interface INode
    {
        public abstract string Stringize(string indent = "");
    }
}
