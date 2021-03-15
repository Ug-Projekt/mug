using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class CompTimeDeclaredExpression : INode
    {
        public string NodeKind => "CompTimeWhen";
        public Token Symbol { get; set; }
        public Range Position { get; set; }
    }
}
