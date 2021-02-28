using Newtonsoft.Json;
using System;

namespace Mug.Models.Parser
{
    public interface INode
    {
        public string NodeKind { get; }
        [JsonIgnore]
        public abstract Range Position { get; set; }
    }
}
