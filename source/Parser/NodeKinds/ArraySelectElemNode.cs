using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class ArraySelectElemNode : INode
    {
        public INode Left { get; set; }
        public INode IndexExpression { get; set; }
        public Range Position { get; set; }
    }
}
