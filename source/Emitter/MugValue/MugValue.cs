using LLVMSharp.Interop;
using Mug.Models.Lexer;
using System;

namespace Mug.MugValueSystem
{
    public struct MugValue
    {
        public MugValueType Type { get; set; }
        public LLVMValueRef LLVMValue { get; set; }
        public bool IsPublic { get; set; }

        public static MugValue From(LLVMValueRef value, MugValueType type, bool ispublic = false)
        {
            return new MugValue() { IsPublic = ispublic, LLVMValue = value, Type = type };
        }

        public static MugValue Struct(LLVMValueRef structure, MugValueType type, bool ispublic)
        {
            return From(structure, type, ispublic);
        }

        public static MugValue Enum(MugValueType enumerated, bool ispublic)
        {
            return From(new LLVMValueRef(), enumerated, ispublic);
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
    }
}
