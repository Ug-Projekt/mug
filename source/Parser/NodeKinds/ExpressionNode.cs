using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class ExpressionNode : INode
    {
        public INode Left { get; set; }
        public INode Rigth { get; set; }
        public Boolean HasSingleValue
        {
            get
            {
                return SingleValue != null;
            }
        }
        public Object SingleValue { get; set; }
        public OperatorKind Operator { get; set; }
        public Range Position { get; set; }
        public string Stringize(string indent = "")
        {
            return indent+"ExpressionNode: "+(!HasSingleValue ? $"(({Position.Start}:{Position.End}) Left:\n{Left.Stringize(indent+"   ")},\n{indent}Rigth:\n{Rigth.Stringize(indent+"   ")})" : $"SingleValue:\n{SingleValue}");
        }
    }
}
