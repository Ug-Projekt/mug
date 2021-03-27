using LLVMSharp.Interop;
using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds.Statements;
using System;

namespace Mug.MugValueSystem
{
    public struct MugValue
    {
        public MugValueType Type { get; set; }
        public LLVMValueRef LLVMValue { get; set; }
        public bool IsConst { get; set; }

        public static MugValue From(LLVMValueRef value, MugValueType type, bool isconst = false)
        {
            return new MugValue() { LLVMValue = value, Type = type, IsConst = isconst };
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

        public static MugValue EnumError(EnumErrorStatement enumerror)
        {
            return From(new LLVMValueRef(), MugValueType.EnumError(enumerror));
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
