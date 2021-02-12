using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class FunctionNode : INode
    {
        public String Name { get; set; }
        public MugType Type { get; set; }
        public ParameterListNode ParameterList { get; set; } = new();
        private List<Token> _genericTypes { get; set; } = new();
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
    }
}
