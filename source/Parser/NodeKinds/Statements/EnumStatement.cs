using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.MugValueSystem;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class EnumStatement : INode
    {
        public string NodeKind => "Struct";
        public Pragmas Pragmas { get; set; }
        public MugType BaseType { get; set; }
        public string Name { get; set; }
        private List<EnumMemberNode> _body { get; set; } = new();
        public EnumMemberNode[] Body
        {
            get
            {
                return _body.ToArray();
            }
        }
        public Range Position { get; set; }
        public TokenKind Modifier { get; set; }

        public void AddMember(EnumMemberNode field)
        {
            _body.Add(field);
        }

        public Range GetMemberPositionFromName(string name)
        {
            for (int i = 0; i < _body.Count; i++)
                if (_body[i].Name == name)
                    return _body[i].Position;

            throw new();
        }

        public MugValue GetMemberValueFromName(MugValueType enumerated, MugValueType enumeratedBaseType, string name, Range position, LocalGenerator localgenerator)
        {
            for (int i = 0; i < _body.Count; i++)
                if (_body[i].Name == name)
                    return MugValue.EnumMember(enumerated, localgenerator.ConstToMugConst(_body[i].Value, _body[i].Position, true, enumeratedBaseType).LLVMValue);

            localgenerator.Error(position, "Enum `", Name, "` does not contain a definition for `", name, "`");

            throw new(); // unreachable
        }

        public bool ContainsMemberWithName(string name)
        {
            for (int i = 0; i < _body.Count; i++)
                if (_body[i].Name == name)
                    return true;

            return false;
        }
    }
}
