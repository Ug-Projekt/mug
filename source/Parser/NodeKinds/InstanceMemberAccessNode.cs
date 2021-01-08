using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class InstanceMemberAccessNode : INode
    {
        public MemberAccessNode Members { get; set; }
        public INode Instance { get; set; }
        public Range Position { get; set; }
        public string Stringize(string indent = "")
        {
            return indent+$"IstanceMemberAccessNode: {{\n{indent}   Instance: {{\n{Instance.Stringize(indent + "      ")}\n{indent}   }}\n{indent}   Members: {{\n{Members.Stringize(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
        public override string ToString()
        {
            return Stringize();
        }
    }
}
