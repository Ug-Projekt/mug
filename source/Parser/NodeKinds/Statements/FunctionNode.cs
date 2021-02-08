using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public enum Modifier
    {
        Private,
        Public,
        Instance
    }
    public class FunctionNode : INode
    {
        public String Name { get; set; }
        public MugType Type { get; set; }
        public Modifier Modifier { get; set; }
        public ParameterListNode ParameterList { get; set; } = new();
        List<Token> _genericTypes { get; set; } = new();
        public Token[] GenericTypes
        {
            get
            {
                return _genericTypes.ToArray();
            }
        }
        public Boolean IsGeneric
        {
            get
            {
                return GenericTypes.Length > 0;
            }
        }
        public void SetGenericTypes(List<Token> types)
        {
            _genericTypes = types;
        }
        public BlockNode Body { get; set; } = new();
        public Range Position { get; set; }
        public string Dump(string indent = "")
        {
            string types = "";
            for (int i = 0; i < _genericTypes.Count; i++)
                types += _genericTypes[i].Dump(indent + "      ") + ",\n";
            return indent+$"FunctionNode: {{\n{indent}   Type: {{\n{Type.Dump(indent + "      ")}\n{indent}   }},\n{indent}   Name: {Name},\n{indent}   Modifier: {Modifier},\n{indent}   ParameterList: {{\n{ParameterList.Dump(indent+"      ")}\n{indent}   }},\n{indent}   IsGeneric: {IsGeneric}{(IsGeneric ? $",\n{indent}   GenericType: {{\n{types}{indent}   }}" : "")},\n{indent}   Body: {{\n{Body.Dump(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
