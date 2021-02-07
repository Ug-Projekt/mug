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
        public LLVMBuilderRef Builder { get; private set; } = CreateBuilder();

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
            var second = Pop();
            Load(BuildAdd(Builder, Pop(), second, ""));
        }
        public void Sub()
        {
            var second = Pop();
            Load(BuildSub(Builder, Pop(), second, ""));
        }
        public void Mul()
        {
            var second = Pop();
            Load(BuildMul(Builder, Pop(), second, ""));
        }
        public void Div()
        {
            var second = Pop();
            Load(BuildSDiv(Builder, Pop(), second, ""));
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
        public void LoadFromMemory(string name)
        {
            Load(BuildLoad(Builder, GetFromMemory(name), ""));
        }
        public void Ret()
        {
            BuildRet(Builder, Pop());
        }
        public void RetVoid()
        {
            BuildRetVoid(Builder);
        }
    }
}
