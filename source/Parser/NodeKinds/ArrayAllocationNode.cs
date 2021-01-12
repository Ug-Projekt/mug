using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class ArrayAllocationNode : INode
    {
        public INode Type { get; set; }
        public INode Size { get; set; }
        List<INode> _body { get; set; } = new();
        public INode[] Body
        {
            get
            {
                return _body.ToArray();
            }
        }
        public Range Position { get; set; }
        public void AddArrayElement(INode element)
        {
            _body.Add(element);
        }
        public string Stringize(string indent = "")
        {
            string nodes = "";
            for (int i = 0; i < _body.Count; i++)
                nodes += indent+"      ArrayElement["+i+"] {\n"+_body[i].Stringize(indent+"         ")+"\n"+indent+"      },\n";
            return indent+$"ArrayAllocationNode: {{\n{indent}   Size: {{\n{Size.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   Type: {{\n{indent}      {Type.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   Body: {{\n{nodes}\n{indent}   }}\n{indent}}}";
        }
    }
}
