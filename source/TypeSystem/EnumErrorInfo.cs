using LLVMSharp.Interop;
using Mug.MugValueSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.TypeSystem
{
    public class EnumErrorInfo
    {
        public string Name { get; set; }
        public MugValueType ErrorType { get; set; }
        public MugValueType SuccessType { get; set; }
        public LLVMTypeRef LLVMValue { get; set; }
    }
}
