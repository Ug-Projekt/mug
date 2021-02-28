using System;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class ReturnStatement : INode
    {
        public string NodeKind => "Return";
        public INode Body { get; set; }
        public Range Position { get; set; }
        public bool IsVoid()
        {
            return Body is null;
        }
    }
}
