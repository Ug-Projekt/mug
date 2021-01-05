using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public interface IStatement: INode
    {
        public abstract Range Position { get; set; }
    }
}
