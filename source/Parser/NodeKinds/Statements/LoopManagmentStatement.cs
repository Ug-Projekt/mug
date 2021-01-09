using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class LoopManagmentStatement : IStatement
    {
        public Token Managment { get; set; }
        public Range Position { get; set; }
        public string Stringize(string indent = "")
        {
            return indent + $"LoopManagmentStatement: {{\n{indent}   {Managment.Stringize(indent+"   ")}\n{indent}   }}";
        }
    }
}
