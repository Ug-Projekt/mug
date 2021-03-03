using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.MugValueSystem
{
    public struct MugValueType
    {
        public LLVMTypeRef LLVMType { get; set; }
        public MugValueTypeKind TypeKind { get; set; }

        public static MugValueType From(LLVMTypeRef type, MugValueTypeKind kind)
        {
            return new MugValueType() { LLVMType = type, TypeKind = kind };
        }

        public static MugValueType Bool => From(LLVMTypeRef.Int1, MugValueTypeKind.Bool);
        public static MugValueType Int8 => From(LLVMTypeRef.Int8, MugValueTypeKind.Int8);
        public static MugValueType Int32 => From(LLVMTypeRef.Int32, MugValueTypeKind.Int32);
        public static MugValueType Int64 => From(LLVMTypeRef.Int64, MugValueTypeKind.Int64);
        public static MugValueType Void => From(LLVMTypeRef.Void, MugValueTypeKind.Void);
        public static MugValueType Char => From(LLVMTypeRef.Int8, MugValueTypeKind.Char);
        public static MugValueType String => From(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), MugValueTypeKind.String);

        public override string ToString()
        {
            return TypeKind switch
            {
                MugValueTypeKind.Bool => "u1",
                MugValueTypeKind.Int8 => "u8",
                MugValueTypeKind.Int32 => "i32",
                MugValueTypeKind.Int64 => "i64",
                MugValueTypeKind.Void => "?",
                MugValueTypeKind.Char => "chr",
                MugValueTypeKind.String => "str"
            };
        }

        public bool MatchAnyTypeOfIntType()
        {
            return
                LLVMType == LLVMTypeRef.Int1 ||
                LLVMType == LLVMTypeRef.Int8 ||
                LLVMType == LLVMTypeRef.Int32 ||
                LLVMType == LLVMTypeRef.Int64;
        }

        public bool MatchIntType()
        {
            return
                TypeKind == MugValueTypeKind.Int8 ||
                TypeKind == MugValueTypeKind.Int32 ||
                TypeKind == MugValueTypeKind.Int64;
        }
    }
}
