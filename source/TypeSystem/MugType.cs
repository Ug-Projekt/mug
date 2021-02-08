using Mug.Compilation;
using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.TypeSystem
{
    public struct MugType
    {
        public TypeKind Kind { get; set; }
        public object BaseType { get; set; }
        public MugType(TypeKind type, object baseType = null)
        {
            Kind = type;
            BaseType = baseType;
        }
        public static MugType FromToken(Token t)
        {
            return t.Kind switch
            {
                TokenKind.KeyTstr => new MugType(TypeKind.String),
                TokenKind.KeyTchr => new MugType(TypeKind.Char),
                TokenKind.KeyTbool => new MugType(TypeKind.Bool),
                TokenKind.KeyTi8 => new MugType(TypeKind.Int8),
                TokenKind.KeyTi32 => new MugType(TypeKind.Int32),
                TokenKind.KeyTi64 => new MugType(TypeKind.Int64),
                TokenKind.KeyTu8 => new MugType(TypeKind.UInt8),
                TokenKind.KeyTu32 => new MugType(TypeKind.UInt32),
                TokenKind.KeyTu64 => new MugType(TypeKind.UInt64),
                TokenKind.KeyTVoid => new MugType(TypeKind.Void),
                TokenKind.Identifier => new MugType(TypeKind.Struct),
                _ => Error(t.Kind.ToString())
            };
        }
        static MugType Error(string kind)
        {
            CompilationErrors.Throw("´", kind, "´ is not a type");
            return new();
        }
        public string Dump(string indent = "")
        {
            return $"Type: {{\n{indent}   Kind: {Kind},\n{indent}   BaseType: {(BaseType is not null ? BaseType : "")}\n{indent}}}";
        }
        public override string ToString()
        {
            return Dump();
        }
    }
}
