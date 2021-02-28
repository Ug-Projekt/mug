using Mug.TypeSystem;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class CallStatement : INode
    {
        public string NodeKind => "Call";
        public NodeBuilder Parameters { get; set; } = new();
        public INode Name { get; set; }

        private List<MugType> _genericTypes { get; set; } = new();
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
