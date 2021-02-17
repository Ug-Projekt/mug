using Newtonsoft.Json;
using System;

namespace Mug.Models.Parser
{
    public interface INode
    {
        [JsonIgnore]
        public abstract Range Position { get; set; }
    }
}
