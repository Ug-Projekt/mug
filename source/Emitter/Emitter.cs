using LLVMSharp;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using static LLVMSharp.LLVM;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        public LLVMBuilderRef Builder { get; private set; } = CreateBuilder();

        readonly Stack<LLVMValueRef> _stack = new();
        readonly Dictionary<string, LLVMValueRef> _memory = new();
        readonly IRGenerator _generator;

        public static readonly LLVMBool ConstLLVMFalse = new LLVMBool(0);
        public static readonly LLVMBool ConstLLVMTrue = new LLVMBool(1);

        public MugEmitter(IRGenerator generator)
        {
            _generator = generator;
        }

        public void Load(LLVMValueRef value)
        {
            _stack.Push(value);
        }
        public LLVMValueRef Pop()
        {
            return _stack.Pop();
        }
        public LLVMValueRef Peek()
        {
            return _stack.Peek();
        }
        public void Duplicate()
        {
            Load(Peek());
        }
        public LLVMTypeRef PeekType()
        {
            return Peek().TypeOf();
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
            _memory.TryAdd(name, value);
        }
        public void DeclareVariable(VariableStatement variable)
        {
            DeclareVariable(variable.Name, variable.Type, variable.Position);
        }
        public void DeclareVariable(string name, MugType type, Range position)
        {
            if (IsDeclared(name))
                _generator.Error(position, "Variable already declared");
            SetMemory(name, BuildAlloca(Builder, _generator.TypeToLLVMType(type, position), name));
        }
        public void StoreVariable(string name)
        {
            BuildStore(Builder, Pop(), GetFromMemory(name));
        }
        bool IsDeclared(string name)
        {
            return _memory.ContainsKey(name);
        }
        public void LoadFromMemory(VariableStatement variable)
        {
            LoadFromMemory(variable.Name, variable.Position);
        }
        public void LoadFromMemory(string name, Range position)
        {
            if (!IsDeclared(name))
                _generator.Error(position, "Undeclared variable");
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
        public void NegInt()
        {
            Load(BuildNeg(Builder, Pop(), ""));
        }
        public void NegBool()
        {
            Load(BuildNot(Builder, Pop(), ""));
        }
    }
}
