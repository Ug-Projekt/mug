using LLVMSharp.Interop;
using Mug.Compilation;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;

namespace Mug.MugValueSystem
{
    public struct MugValueType
    {
        private object BaseType { get; set; }
        public MugValueTypeKind TypeKind { get; set; }
        public LLVMTypeRef LLVMType
        {
            get
            {
                return TypeKind switch
                {
                    MugValueTypeKind.Pointer or
                    MugValueTypeKind.Reference => LLVMTypeRef.CreatePointer(((MugValueType)BaseType).LLVMType, 0),
                    MugValueTypeKind.Struct => GetStructure().LLVMValue,
                    MugValueTypeKind.Enum => GetEnumInfo().Item1.LLVMType,
                    MugValueTypeKind.Array => LLVMTypeRef.CreatePointer(((MugValueType)BaseType).LLVMType, 0),
                    MugValueTypeKind.EnumErrorDefined => GetEnumErrorDefined().LLVMValue,
                    MugValueTypeKind.EnumError => LLVMTypeRef.Int8,
                    _ => (LLVMTypeRef)BaseType,
                };
            }
        }

        public MugValueType PointerBaseElementType
        {
            get
            {
                return (MugValueType)BaseType;
            }
        }

        public MugValueType ArrayBaseElementType
        {
            get
            {
                if (TypeKind == MugValueTypeKind.String)
                    return Char;

                if (TypeKind == MugValueTypeKind.Array)
                    return (MugValueType)BaseType;

                throw new("");
            }
        }

        public int Size(int sizeofpointer)
        {
            return TypeKind switch
            {
                MugValueTypeKind.Bool => 1,
                MugValueTypeKind.Int8 => 1,
                MugValueTypeKind.Int32 => 4,
                MugValueTypeKind.Int64 => 8,
                MugValueTypeKind.Void => 0,
                MugValueTypeKind.Char => 1,
                MugValueTypeKind.String => sizeofpointer,
                MugValueTypeKind.Unknown => sizeofpointer,
                MugValueTypeKind.Struct => GetStructure().Size(sizeofpointer),
                MugValueTypeKind.Pointer or
                MugValueTypeKind.Reference => sizeofpointer,
                MugValueTypeKind.Enum => GetEnumInfo().Item1.Size(sizeofpointer),
                MugValueTypeKind.Array => sizeofpointer,
                MugValueTypeKind.Function => sizeofpointer
            };
        }

        public static MugValueType From(LLVMTypeRef type, MugValueTypeKind kind)
        {
            return new MugValueType() { BaseType = type, TypeKind = kind };
        }

        public static MugValueType Pointer(MugValueType type)
        {
            return new MugValueType() { TypeKind = MugValueTypeKind.Pointer, BaseType = type };
        }

        public static MugValueType Struct(string name, MugValueType[] body, string[] structure, Range[] positions)
        {
            return new MugValueType()
            {
                BaseType = new StructureInfo()
                {
                    Name = name,
                    FieldNames = structure,
                    FieldPositions = positions,
                    FieldTypes = body,
                    LLVMValue = LLVMTypeRef.CreateStruct(GetLLVMTypes(body), false)
                },
                TypeKind = MugValueTypeKind.Struct
            };
        }

        private static LLVMTypeRef[] GetLLVMTypes(MugValueType[] body)
        {
            var result = new LLVMTypeRef[body.Length];

            for (int i = 0; i < body.Length; i++)
                result[i] = body[i].LLVMType;

            return result;
        }

        public static MugValueType Enum(MugValueType basetype, EnumStatement enumstatement)
        {
            return new MugValueType { TypeKind = MugValueTypeKind.Enum, BaseType = (basetype, enumstatement) };
        }

        public static MugValueType Array(MugValueType basetype)
        {
            return new MugValueType { TypeKind = MugValueTypeKind.Array, BaseType = basetype };
        }

        public static MugValueType Bool => From(LLVMTypeRef.Int1, MugValueTypeKind.Bool);
        public static MugValueType Int8 => From(LLVMTypeRef.Int8, MugValueTypeKind.Int8);
        public static MugValueType Int32 => From(LLVMTypeRef.Int32, MugValueTypeKind.Int32);
        public static MugValueType Int64 => From(LLVMTypeRef.Int64, MugValueTypeKind.Int64);
        public static MugValueType Void => From(LLVMTypeRef.Void, MugValueTypeKind.Void);
        public static MugValueType Char => From(LLVMTypeRef.Int8, MugValueTypeKind.Char);
        public static MugValueType String => From(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), MugValueTypeKind.String);
        public static MugValueType Unknown => From(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), MugValueTypeKind.Unknown);

        public static MugValueType Function(MugValueType[] paramTypes, MugValueType retType)
        {
            return new MugValueType() { TypeKind = MugValueTypeKind.Function, BaseType = (paramTypes, retType) };
        }

        public static MugValueType EnumError(EnumErrorStatement enumerror)
        {
            return new MugValueType() { TypeKind = MugValueTypeKind.EnumError, BaseType = enumerror };
        }

        public static MugValueType EnumErrorDefined(EnumErrorInfo enumerrorInfo)
        {
            return new MugValueType() { BaseType = enumerrorInfo, TypeKind = MugValueTypeKind.EnumErrorDefined };
        }

        public static MugValueType Reference(MugValueType type)
        {
            return new MugValueType() { TypeKind = MugValueTypeKind.Reference, BaseType = type };
        }

        public override string ToString()
        {
            return TypeKind switch
            {
                MugValueTypeKind.Bool => "u1",
                MugValueTypeKind.Int8 => "u8",
                MugValueTypeKind.Int32 => "i32",
                MugValueTypeKind.Int64 => "i64",
                MugValueTypeKind.Void => "void",
                MugValueTypeKind.Char => "chr",
                MugValueTypeKind.String => "str",
                MugValueTypeKind.Unknown => "unknown",
                MugValueTypeKind.Struct => GetStructure().Name,
                MugValueTypeKind.Pointer => $"*{BaseType}",
                MugValueTypeKind.Reference => $"&{BaseType}",
                MugValueTypeKind.Enum => GetEnum().Name,
                MugValueTypeKind.Array => $"[{BaseType}]",
                MugValueTypeKind.Function => $"func({string.Join(", ", GetFunction().Item1)}): {GetFunction().Item2}",
                MugValueTypeKind.EnumError => $"{GetEnumError().Name}",
                MugValueTypeKind.EnumErrorDefined => $"{GetEnumErrorDefined().Name}"
            };
        }

        public bool MatchSameIntType(MugValueType st)
        {
            return MatchIntType() && st.MatchIntType() && TypeKind == st.TypeKind;
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

        public bool IsIndexable()
        {
            return TypeKind == MugValueTypeKind.Array || TypeKind == MugValueTypeKind.String;
        }

        public bool IsPointer()
        {
            return TypeKind == MugValueTypeKind.Pointer;
        }

        public bool IsFunction()
        {
            return TypeKind == MugValueTypeKind.Function;
        }

        public StructureInfo GetStructure()
        {
            return (StructureInfo)BaseType;
        }

        public EnumErrorStatement GetEnumError()
        {
            return (EnumErrorStatement)BaseType;
        }

        public EnumErrorInfo GetEnumErrorDefined()
        {
            return (EnumErrorInfo)BaseType;
        }

        public (MugValueType[], MugValueType) GetFunction()
        {
            return ((MugValueType[], MugValueType))BaseType;
        }

        private (MugValueType, EnumStatement) GetEnumInfo()
        {
            return ((MugValueType, EnumStatement))BaseType;
        }

        public EnumStatement GetEnum()
        {
            return GetEnumInfo().Item2;
        }

        public bool IsSameEnumOf(MugValueType st)
        {
            return IsEnum() && st.IsEnum() && GetEnum().Equals(st.GetEnum());
        }

        public bool IsEnum()
        {
            return BaseType is (MugValueType, EnumStatement);
        }

        public bool IsEnumErrorDefined()
        {
            return BaseType is EnumErrorInfo;
        }

        public bool IsEnumError()
        {
            return BaseType is EnumErrorStatement;
        }

        public bool RawEquals(MugValueType type)
        {
            return base.Equals(type);
        }

        public override bool Equals(object obj)
        {
            if (obj is not MugValueType type)
                return false;

            if ((TypeKind == MugValueTypeKind.Reference && type.TypeKind == MugValueTypeKind.Pointer) ||
                (TypeKind == MugValueTypeKind.Pointer && type.TypeKind == MugValueTypeKind.Reference))
                return BaseType.Equals(type.BaseType);

            return base.Equals(type);
        }

        public bool IsStructure()
        {
            return BaseType is StructureInfo;
        }
    }
}
