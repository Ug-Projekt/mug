using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class TypeStatement : INode 
    {
        public String Name { get; set; }
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
        List<FieldNode> _body { get; set; } = new();
        public FieldNode[] Body
        {
            get
            {
                return _body.ToArray();
            }
        }
        public Range Position { get; set; }
        public void AddGenericType(Token type)
        {
            _genericTypes.Add(type);
        }
        public void AddField(FieldNode field)
        {
            _body.Add(field);
        }
        public string Dump(string indent = "")
        {
            string nodes = "";
            for (int i = 0; i < _body.Count; i++)
                nodes += _body[i].Dump(indent + "      ") + ",\n";
            string types = "";
            for (int i = 0; i < _genericTypes.Count; i++)
                types += indent + "      " + _genericTypes[i].Dump(indent + "      ") + ",\n";
            return indent+ $"TypeStatement: {{\n{indent}   Name: {Name},\n{indent}   Body: {{\n{nodes}\n{indent}   }},\n{indent}   IsGeneric: {IsGeneric}{(IsGeneric ? $",\n{indent}   GenericType: {{\n{types}{indent}   }}" : "")}\n{indent}}}";
        }
    }
}
