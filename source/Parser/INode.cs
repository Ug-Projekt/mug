using Newtonsoft.Json;
using System;

namespace Mug.Models.Parser
{
    public interface INode
    {
        public string NodeKind { get; }
        [JsonIgnore]
        public abstract Range Position { get; set; }

        public string Dump()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public interface IStatement : INode
    {
    }
}
