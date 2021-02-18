﻿using Mug.Compilation;
using Mug.Models.Lexer;
using System;

namespace Mug.TypeSystem
{
    public struct MugType
    {
        public TypeKind Kind { get; set; }
        public object BaseType { get; set; }

        /// <summary>
        /// basetype is used when kind is a non primitive type, a pointer or an array
        /// </summary>
        public MugType(TypeKind type, object baseType = null)
        {
            Kind = type;
            BaseType = baseType;
        }

        /// <summary>
        /// converts a keyword token into a type
        /// </summary>
        public static MugType FromToken(Token t)
        {
            return t.Kind switch
            {
                TokenKind.KeyTstr => new MugType(TypeKind.String),
                TokenKind.KeyTchr => new MugType(TypeKind.Char),
                TokenKind.KeyTbool => new MugType(TypeKind.Bool),
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

        /// <summary>
        /// a short way of allocating with new operator
        /// </summary>
        public static MugType Automatic()
        {
            return new MugType(TypeKind.Auto);
        }

        /// <summary>
        /// used for implicit type specification in var, const declarations
        /// </summary>
        public bool IsAutomatic()
        {
            return Kind == TypeKind.Auto;
        }

        /// <summary>
        /// returns a string reppresentation of the type
        /// </summary>
        public override string ToString()
        {
            return Kind switch
            {
                TypeKind.Auto => "auto",
                TypeKind.Array => $"[{BaseType}]",
                TypeKind.Bool => "u1",
                TypeKind.Char => "chr",
                TypeKind.Struct => BaseType.ToString(),
                TypeKind.Int32 => "i32",
                TypeKind.Int64 => "i64",
                TypeKind.UInt8 => "u8",
                TypeKind.UInt32 => "u32",
                TypeKind.UInt64 => "u64",
                TypeKind.Pointer => $"ptr {BaseType}",
                TypeKind.String => "str",
                TypeKind.Void => "?",
            };
        }

        public bool IsInt()
        {
            return
                Kind == TypeKind.Int32  ||
                Kind == TypeKind.Int64  ||
                Kind == TypeKind.UInt8  ||
                Kind == TypeKind.UInt32 ||
                Kind == TypeKind.UInt64;
        }
    }
}
