using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class CallStatement : INode
    {
        public INode Name { get; set; }
        public NodeBuilder Parameters { get; set; }
        public Boolean HasParameters
        {
            get
            {
                return Parameters != null;
            }
        }
        List<INode> _genericTypes { get; set; } = new();
        public INode[] GenericTypes
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
        public void SetGenericTypes(List<INode> types)
        {
            _genericTypes = types;
        }
        public Range Position { get; set; }
        public string Stringize(string indent = "")
        {
            string types = "";
            for (int i = 0; i < _genericTypes.Count; i++)
                types += _genericTypes[i].Stringize(indent + "      ") + ",\n";
            return indent + $"CallStatement: {{\n{indent}   Name: {{\n{Name.Stringize(indent + "      ")}\n{indent}   }},\n{indent}   Parameters: {{\n{(HasParameters ? Parameters.Stringize(indent+"      ") : "")}\n{indent}   }},\n{indent}   IsGeneric: {IsGeneric}{(IsGeneric ? $",\n{indent}   GenericType: {{\n{types}{indent}   }}" : "")}\n{indent}}}";
        }
    }
}
