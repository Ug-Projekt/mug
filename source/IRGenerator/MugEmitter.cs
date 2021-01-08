using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        public String ModuleName { get; }
        public LLVMModuleRef LLVMModule { get; }
        public MugEmitter(string moduleName)
        {
            ModuleName = moduleName;
            LLVMModule = LLVM.ModuleCreateWithName(moduleName);
        }
        public void DefineFunction()
        {
            LLVMTypeRef[] args = { };
            var main = LLVM.AddFunction(LLVMModule, "main", LLVM.FunctionType(LLVM.Int32Type(), args, false));
            var entry = LLVM.AppendBasicBlock(main, "entry");
            LLVMBuilderRef builder = LLVM.CreateBuilder();
            LLVM.PositionBuilderAtEnd(builder, entry);
        }
    }
}
