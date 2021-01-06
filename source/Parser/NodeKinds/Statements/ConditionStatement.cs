using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct ConditionStatement : IStatement 
    {
        public TokenKind Kind { get; set; }
        public INode Expression { get; set; }
        public BlockNode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+ "ConditionStatement: " + $"(({Position.Start}:{Position.End}) Kind: {Kind}, Expression:\n{(Kind != TokenKind.KeyELSE ? Expression.Stringize(indent+"   ") : "(empty)")},\n{indent}Body:\n{Body.Stringize(indent+"   ")})";
        }
    }
}
