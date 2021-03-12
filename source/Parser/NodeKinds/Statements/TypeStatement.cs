using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class TypeStatement : INode
    {
        public string NodeKind => "Struct";
        public Pragmas Pragmas { get; set; }
        public string Name { get; set; }
        public List<Token> Generics { get; set; } = new();
        public List<FieldNode> Body { get; set; } = new();
        public Range Position { get; set; }
        public TokenKind Modifier { get; set; }

        public int GetFieldIndexFromName(string name)
        {
            for (int i = 0; i < Body.Count; i++)
                if (Body[i].Name == name)
                    return i;

            throw new();
        }

        public MugType GetFieldTypeFromName(string name)
        {
            for (int i = 0; i < Body.Count; i++)
                if (Body[i].Name == name)
                    return Body[i].Type;

            throw new();
        }

        public Range GetFieldPositionFromName(string name)
        {
            for (int i = 0; i < Body.Count; i++)
                if (Body[i].Name == name)
                    return Body[i].Position;

            throw new();
        }

        public bool ContainsFieldWithName(string name)
        {
            for (int i = 0; i < Body.Count; i++)
                if (Body[i].Name == name)
                    return true;

            return false;
        }
    }
}
