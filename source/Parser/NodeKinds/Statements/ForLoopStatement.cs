using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct ForLoopStatement : IStatement 
    {
        public TokenKind Operator { get; set; }
        // VariableStatement, AssignStatement, ForCounterReference
        public INode Counter { get; set; }
        public INode RightExpression { get; set; }
        public BlockNode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent + $"ForLoopStatement: {{\n{indent}   Operator: {Operator},\n{indent}   Counter {{\n{Counter.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   RightExpression {{\n{RightExpression.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   Body {{\n{Body.Stringize(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
