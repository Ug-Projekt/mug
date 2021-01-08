using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class MemberAccessNode : INode
    {
        List<Token> members { get; set; } = null;
        public Token[] Members
        {
            get
            {
                return members.ToArray();
            }
        }
        public void Add(Token member)
        {
            if (members is null)
                members = new List<Token>();
            members.Add(member);
        }
        public void PreAdd(Token member)
        {
            if (members is null)
                members = new List<Token>();
            members.Insert(0, member);
        }
        public Range Position { get; set; }
        public string Stringize(string indent = "")
        {
            return indent+$"MemberAccessNode: {{\n{indent}   Members: {{\n{indent}      {string.Join(",\n"+indent+"      ", members)}\n{indent}   }}\n{indent}}}";
        }
        public override string ToString()
        {
            return Stringize();
        }
    }
}
