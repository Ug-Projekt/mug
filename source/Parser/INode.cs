using System;

namespace Mug.Models.Parser
{
    public interface INode
    {
        public abstract Range Position { get; set; }
    }
}
