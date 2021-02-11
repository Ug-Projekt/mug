using Mug.Compilation;
using Mug.Models.Lexer;

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

        private static MugType Error(string kind)
        {
            CompilationErrors.Throw("´", kind, "´ is not a type");
            return new();
        }
        public static MugType Automatic()
        {
            return new MugType(TypeKind.Auto);
        }
        public bool IsAutomatic()
        {
            return Kind == TypeKind.Auto;
        }
    }
}
