using LLVMSharp;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static LLVMSharp.LLVM;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        public LLVMBuilderRef Builder { get; private set; } = CreateBuilder();

        private readonly Stack<LLVMValueRef> _stack = new();
        private readonly Dictionary<string, LLVMValueRef> _memory = new();
        private readonly IRGenerator _generator;

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

        public LLVMTypeRef PeekType()
        {
            return Peek().TypeOf();
        }

        public void Add(Range position)
        {
            var exprType = PeekType();

            if (_generator.MatchStringType(exprType))
                Call(_generator.GetSymbol("string_concat", position), 2);
            else
            {
                var second = Pop();

                Load(BuildAdd(Builder, Pop(), second, ""));
            }
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

        public void CastInt(LLVMTypeRef type)
        {
            Load(BuildIntCast(Builder, Pop(), type, ""));
        }

        private LLVMValueRef GetFromMemory(string name)
        {
            return _memory[name];
        }

        private void SetMemory(string name, LLVMValueRef value)
        {
            _memory.TryAdd(name, value);
        }

        public void DeclareVariable(VariableStatement variable)
        {
            DeclareVariable(variable.Name, _generator.TypeToLLVMType(variable.Type, variable.Position), variable.Position);
        }

        public void DeclareVariable(string name, LLVMTypeRef type, Range position)
        {
            if (IsDeclared(name))
                _generator.Error(position, "Variable already declared");

            SetMemory(name, BuildAlloca(Builder, type, name));
        }

        public void StoreVariable(string name)
        {
            BuildStore(Builder, Pop(), GetFromMemory(name));
        }

        private bool IsDeclared(string name)
        {
            return _memory.ContainsKey(name);
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

        /// <summary>
        /// buils an array of the parameters to pass and calls the function
        /// </summary>
        public void Call(LLVMValueRef function, int paramCount)
        {
            var parameters = new LLVMValueRef[paramCount];

            for (int i = 0; i < paramCount; i++)
                parameters[i] = Pop();

            var result = BuildCall(Builder, function, parameters, "");

            if (result.TypeOf().TypeKind != LLVMTypeKind.LLVMVoidTypeKind)
                Load(result);
        }
    }
}
