using LLVMSharp.Interop;
using Mug.Models.Lexer;
using System;

namespace Mug.MugValueSystem
{
    public struct MugValue
    {
        public MugValueType Type { get; set; }
        public LLVMValueRef LLVMValue { get; set; }
        public bool IsReference { get; set; }

        public static MugValue From(LLVMValueRef value, MugValueType type, bool isreference = false)
        {
            return new MugValue() { LLVMValue = value, Type = type, IsReference = isreference };
        }

        public static MugValue Struct(LLVMValueRef structure, MugValueType type)
        {
            return From(structure, type);
        }

        public static MugValue Enum(MugValueType enumerated)
        {
            return From(new LLVMValueRef(), enumerated);
        }

        public static MugValue EnumMember(MugValueType enumerated, LLVMValueRef value)
        {
            return From(value, enumerated);
        }

        public bool IsAllocaInstruction()
        {
            return LLVMValue.IsAAllocaInst.Handle != IntPtr.Zero;
        }

        public bool IsGEP()
        {
            return LLVMValue.IsAGetElementPtrInst.Handle != IntPtr.Zero;
        }

        public bool IsFunction()
        {
            return LLVMValue.IsAFunction.Handle != IntPtr.Zero;
        }

        public bool IsConstant()
        {
            return LLVMValue.IsAConstantInt.Handle != IntPtr.Zero;
        }
    }
}
