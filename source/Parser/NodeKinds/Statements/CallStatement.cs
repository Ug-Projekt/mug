using Mug.Models.Lexer;
using Mug.TypeSystem;
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
        List<MugType> _genericTypes { get; set; } = new();
        public MugType[] GenericTypes
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
        public void SetGenericTypes(List<MugType> types)
        {
            _genericTypes = types;
        }
        public Range Position { get; set; }
    }
}
