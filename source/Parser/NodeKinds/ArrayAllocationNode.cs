using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class ArrayAllocationNode : INode
    {
        public MugType Type { get; set; }
        public INode Size { get; set; }
        List<INode> _body { get; set; } = new();
        public INode[] Body
        {
            get
            {
                return _body.ToArray();
            }
        }
        public Range Position { get; set; }
        public void AddArrayElement(INode element)
        {
            _body.Add(element);
        }
    }
}
