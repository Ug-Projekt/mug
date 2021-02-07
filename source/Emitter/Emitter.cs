using LLVMSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LLVMSharp.LLVM;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        public LLVMBuilderRef Builder = CreateBuilder();
        
        readonly Stack<LLVMValueRef> _stack = new();
        readonly Dictionary<string, LLVMValueRef> _memory = new();

        public static readonly LLVMBool _llvmfalse = new LLVMBool(0);

        public void Load(LLVMValueRef value)
        {
            _stack.Push(value);
        }
        public LLVMValueRef Pop()
        {
            return _stack.Pop();
        }
        public void Duplicate()
        {
            Load(_stack.Peek());
        }
        public void Add()
        {
            Load(BuildAdd(Builder, Pop(), Pop(), "tmpadd"));
        }
        public void Sub()
        {
            Load(BuildSub(Builder, Pop(), Pop(), "tmpsub"));
        }
        public void Mul()
        {
            Load(BuildMul(Builder, Pop(), Pop(), "tmpmul"));
        }
        public void Div()
        {
            Load(BuildSDiv(Builder, Pop(), Pop(), "tmpdiv"));
        }
        LLVMValueRef GetFromMemory(string name)
        {
            return _memory[name];
        }
        void SetMemory(string name, LLVMValueRef value)
        {
            _memory.Add(name, value);
        }
        public void DeclareVariable(string name, LLVMTypeRef type)
        {
            SetMemory(name, BuildAlloca(Builder, type, name));
        }
        public void StoreVariable(string name)
        {
            BuildStore(Builder, Pop(), GetFromMemory(name));
        }
    }
}
