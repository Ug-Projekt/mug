using LLVMSharp.Interop;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.MugValueSystem
{
    struct MugValue
    {
        public MugValueType Type { get; set; }
        public LLVMValueRef LLVMValue { get; set; }

        public static MugValue From(LLVMValueRef value, MugValueType type)
        {
            return new MugValue() { LLVMValue = value, Type = type };
        }

        internal static MugValue Struct(LLVMValueRef structure, MugValueType type)
        {
            return From(structure, type);
        }

        public bool IsAllocaInstruction()
        {
            return LLVMValue.IsAAllocaInst.Handle != IntPtr.Zero;
        }
    }
}
