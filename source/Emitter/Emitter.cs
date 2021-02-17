using LLVMSharp.Interop;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using static LLVMSharp.Interop.LLVM;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        unsafe public LLVMBuilderRef Builder { get; private set; } = CreateBuilder();

        private readonly Stack<LLVMValueRef> _stack = new();
        private readonly Dictionary<string, LLVMValueRef> _memory = new();
        private readonly IRGenerator _generator;

        // implicit function operators
        public const string StringConcatenationIF = "+(i8*, i8*)";
        public const string StringToCharArrayIF = "as chr[](i8*)";

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
            return Peek().TypeOf;
        }

        public void Add()
        {
            var second = Pop();

            Load(Builder.BuildAdd(Pop(), second));
        }

        public void Sub()
        {
            var second = Pop();
            Load(Builder.BuildSub(Pop(), second));
        }

        public void Mul()
        {
            var second = Pop();
            Load(Builder.BuildMul(Pop(), second));
        }

        public void Div()
        {
            var second = Pop();
            Load(Builder.BuildSDiv(Pop(), second));
        }

        public void CastInt(LLVMTypeRef type)
        {
            Load(Builder.BuildIntCast(Pop(), type));
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

            SetMemory(name, Builder.BuildAlloca(type, name));
        }

        public void StoreVariable(string name)
        {
            Builder.BuildStore(Pop(), GetFromMemory(name));
        }

        private bool IsDeclared(string name)
        {
            return _memory.ContainsKey(name);
        }

        public void LoadFromMemory(string name, Range position)
        {
            if (!IsDeclared(name))
                _generator.Error(position, "Undeclared variable");

            Load(Builder.BuildLoad(GetFromMemory(name)));
        }

        public void Ret()
        {
            Builder.BuildRet(Pop());
        }

        public void RetVoid()
        {
            Builder.BuildRetVoid();
        }

        public void NegInt()
        {
            Load(Builder.BuildNeg(Pop()));
        }

        public void NegBool()
        {
            Load(Builder.BuildNot(Pop()));
        }

        /// <summary>
        /// buils an array of the parameters to pass and calls the function
        /// </summary>
        public void Call(LLVMValueRef function, int paramCount)
        {
            var parameters = new LLVMValueRef[paramCount];

            for (int i = 0; i < paramCount; i++)
                parameters[i] = Pop();

            var result = Builder.BuildCall(function, parameters, "");

            if (result.TypeOf.Kind != LLVMTypeKind.LLVMVoidTypeKind)
                Load(result);
        }

        public void ConcatString(Range position)
        {
            Call(_generator.GetSymbol(StringConcatenationIF, position), 2);
        }
    }
}
