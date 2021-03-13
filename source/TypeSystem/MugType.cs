using Mug.Compilation;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.MugValueSystem;
using System;
using System.Collections.Generic;

namespace Mug.TypeSystem
{
    public class MugType : INode
    {
        public string NodeKind => "Type";
        public TypeKind Kind { get; set; }
        public object BaseType { get; set; }
        public Range Position { get; set; }

        /// <summary>
        /// basetype is used when kind is a non primitive type, a pointer or an array
        /// </summary>
        public MugType(Range position, TypeKind type, object baseType = null)
        {
            Kind = type;
            BaseType = baseType;
            Position = position;
        }

        /// <summary>
        /// converts a keyword token into a type
        /// </summary>
        public static MugType FromToken(Token t)
        {
            return t.Kind switch
            {
                TokenKind.KeyTstr => new MugType(t.Position, TypeKind.String),
                TokenKind.KeyTchr => new MugType(t.Position, TypeKind.Char),
                TokenKind.KeyTbool => new MugType(t.Position, TypeKind.Bool),
                TokenKind.KeyTi32 => new MugType(t.Position, TypeKind.Int32),
                TokenKind.KeyTi64 => new MugType(t.Position, TypeKind.Int64),
                TokenKind.KeyTu8 => new MugType(t.Position, TypeKind.UInt8),
                TokenKind.KeyTu32 => new MugType(t.Position, TypeKind.UInt32),
                TokenKind.KeyTu64 => new MugType(t.Position, TypeKind.UInt64),
                TokenKind.KeyTVoid => new MugType(t.Position, TypeKind.Void),
                TokenKind.Identifier => new MugType(t.Position, TypeKind.DefinedType, t.Value),
                _ => Error(t.Kind.ToString())
            };
        }

        private static MugType Error(string kind)
        {
            CompilationErrors.Throw("´", kind, "´ is not a type");
            throw new();
        }

        /// <summary>
        /// a short way of allocating with new operator
        /// </summary>
        public static MugType Automatic(Range position)
        {
            return new MugType(position, TypeKind.Auto);
        }

        /// <summary>
        /// used for implicit type specification in var, const declarations
        /// </summary>
        public bool IsAutomatic()
        {
            return Kind == TypeKind.Auto;
        }

        public Tuple<MugType, List<MugType>> GetGenericStructure()
        {
            return ((Tuple<MugType, List<MugType>>)BaseType);
        }

        public bool IsGeneric()
        {
            return BaseType is Tuple<MugType, List<MugType>>;
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
                TypeKind.DefinedType => BaseType.ToString(),
                TypeKind.GenericDefinedType => GetGenericStructure().Item1.ToString(),
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
                Kind == TypeKind.Int32 ||
                Kind == TypeKind.Int64 ||
                Kind == TypeKind.UInt8 ||
                Kind == TypeKind.UInt32 ||
                Kind == TypeKind.UInt64;
        }

        /// <summary>
        /// the function converts a Mugtype to the corresponding mugvaluetype
        /// </summary>
        public MugValueType ToMugValueType(Range position, IRGenerator generator)
        {
            return Kind switch
            {
                TypeKind.Int32 => MugValueType.Int32,
                TypeKind.UInt8 => MugValueType.Int8,
                TypeKind.Int64 => MugValueType.Int64,
                TypeKind.Bool => MugValueType.Bool,
                TypeKind.Void => MugValueType.Void,
                TypeKind.Char => MugValueType.Char,
                TypeKind.String => MugValueType.String,
                TypeKind.DefinedType => generator.GetSymbol(BaseType.ToString(), position).Type,
                TypeKind.GenericDefinedType => generator.GetGeneric(GetGenericStructure(), position).Type,
                TypeKind.Pointer => MugValueType.Pointer(((MugType)BaseType).ToMugValueType(position, generator)),
                TypeKind.Array => MugValueType.Array(((MugType)BaseType).ToMugValueType(position, generator)),
                _ => generator.NotSupportedType<MugValueType>(Kind.ToString(), position)
            };
        }

        /// <summary>
        /// the function tries to convert a Mugtype to the corresponding mugvaluetype
        /// </summary>
        public bool TryToMugValueType(Range position, IRGenerator generator, out MugValueType type)
        {
            try { type = ToMugValueType(position, generator); return true; } catch { type = MugValueType.Void; return false; }
        }

        public bool IsAllocableTypeNew()
        {
            return Kind == TypeKind.DefinedType || Kind == TypeKind.GenericDefinedType || Kind == TypeKind.Array || Kind == TypeKind.Pointer;
        }
    }
}
