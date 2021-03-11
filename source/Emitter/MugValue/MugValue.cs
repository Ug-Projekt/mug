using LLVMSharp.Interop;
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

        internal static MugValue Struct(LLVMValueRef structure, MugValueType type, bool ispublic = false)
        {
            return From(structure, type, ispublic);
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
